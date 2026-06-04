# Добавление нового Trigger / Action в Puru T23

## Структура файлов

```
Runtime/Script/Trigger/T23_MyTrigger.cs        ← runtime UdonSharpBehaviour
Runtime/Script/Action/T23_MyAction.cs

Editor/Trigger/T23_MyTriggerEditor.cs          ← custom inspector
Editor/Action/T23_MyActionEditor.cs

Runtime/ProgramAsset/Trigger/T23_MyTrigger.asset  ← генерируется _gen_meta_assets.py
Runtime/ProgramAsset/Action/T23_MyAction.asset
```

---

## 1. Runtime Script

### Trigger

```csharp
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Trigger2to3
{
    public class T23_MyTrigger : T23_TriggerBase
    {
        // Публичные поля — сериализуются в Inspector

        // Переопределить нужные VRC-события:
        public override void OnSomeVRCEvent()
        {
            Trigger();  // локальный триггер
        }

        // Если нужен player-aware триггер:
        // public override void OnPlayerEnterTrigger(VRCPlayerApi player)
        // {
        //     AnyPlayerTrigger(player);
        // }
    }
}
```

Нет `Start()`, `Awake()` — базовый класс уже вызывает `OnStart()` / `PostStart()`.  
Если нужна инициализация — переопределить `protected override void OnStart()`.

### Action

```csharp
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Trigger2to3
{
    public class T23_MyAction : T23_ActionBase
    {
        public SomeType[] recievers;  // массив целей (имя recievers — конвенция)

        // Если Action использует DrawToggleOperationField() или DrawBoolOperationField() в Editor —
        // эти два поля ОБЯЗАТЕЛЬНЫ (PropertyBoxField ищет их по имени через FindProperty):
        public T23_PropertyBox propertyBox;
        public bool usePropertyBox;

        public bool toggle;
        public bool operation = true;

        protected override void OnAction()
        {
            if (usePropertyBox && propertyBox)
                operation = propertyBox.value_b;  // читать из PropertyBox

            for (int i = 0; i < recievers.Length; i++)
            {
                if (recievers[i]) Execute(recievers[i]);
            }
        }

        private void Execute(SomeType target)
        {
            // если toggle: инвертировать состояние
            // иначе: применить operation
        }
    }
}
```

`randomAvg`, `priority`, `groupID`, `title` — уже есть в базовых классах, не добавлять повторно.

> **Важно:** `propertyBox` / `usePropertyBox` нужны только если Editor вызывает `DrawToggleOperationField()` или `DrawBoolOperationField()`. Без них — NullReferenceException в инспекторе. Без этих полей в самом классе эти методы не вызывать.

---

## 2. Editor Inspector

```csharp
#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;

namespace Trigger2to3
{
    [CustomEditor(typeof(T23_MyTrigger))]  // или T23_MyAction
    internal class T23_MyTriggerEditor : T23_TriggerEditorBase  // или T23_ActionEditorBase
    {
        protected override void DrawFields()
        {
            // Варианты:

            // Массив recievers (Action):
            // DrawRecieversList();
            // DrawRecieversList("myCustomFieldName");

            // Обычное поле:
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("fieldName"));

            // Toggle / SetActive паттерн (toggle + operation):
            // DrawToggleOperationField();

            // Bool-операция (operation: true/false):
            // DrawBoolOperationField();

            // Поле с переключением на PropertyBox:
            // PropertyBoxField("operation", "propertyBox", "usePropertyBox");
            // PropertyBoxField("value", "propertyBox", "usePropertyBox", () => { /* кастомный UI */ });
        }
    }
}
#endif
```

Базовый класс (`T23_ModuleEditorBase`) сам рисует заголовок, groupID, кнопку Master.  
`DrawFields()` — только специфичные поля модуля.

---

## 3. Категория в меню Master

Файл: `Editor/Common/T23_EditorUtility.cs`

Добавить строку в нужный словарь:

```csharp
// Для Trigger:
private static readonly Dictionary<string, string> s_TriggerCategories = new Dictionary<string, string>
{
    // ...
    { "T23_MyTrigger",  "CategoryName" },  // ← добавить
};

// Для Action:
private static readonly Dictionary<string, string> s_ActionCategories = new Dictionary<string, string>
{
    // ...
    { "T23_MyAction",   "CategoryName" },  // ← добавить
};
```

Если имя не в словаре — модуль попадёт в `Other`. Это работает, но лучше прописать явно.

Существующие категории Trigger: `Lifecycle`, `Interaction`, `Input`, `Player`, `Network`, `Station`, `Timer`, `Video`, `UI`, `Physics`, `MIDI`, `Logic`  
Существующие категории Action: `GameObject`, `Object Pool`, `Renderer`, `Animation`, `Physics`, `Audio`, `Player`, `Teleport`, `Pickup`, `UI`, `System`, `Logic`, `Deprecated`

---

---

## Интеграционный модуль (опциональная зависимость)

Для модулей, зависящих от опционального пакета (LTCGI, AudioLink, ProTV и т.д.) — специальный паттерн.  
Референс: `T23_SetLtcgiState` (LTCGI).

### Структура файлов

```
Runtime/Script/Integration/<Pkg>/
    T23_My<Pkg>Action.cs                         ← runtime, обёрнут в #if
    Trigger2to3.<Pkg>.Runtime.asmdef             ← conditional asmdef
    Trigger2to3.<Pkg>.Runtime.asmdef.meta
    Trigger2to3.<Pkg>.Runtime.asset              ← UdonSharpAssemblyDefinition (создать вручную)
    Trigger2to3.<Pkg>.Runtime.asset.meta

Editor/Integration/<Pkg>/
    T23_My<Pkg>ActionEditor.cs                   ← editor, обёрнут в #if
    Trigger2to3.<Pkg>.Editor.asmdef
    Trigger2to3.<Pkg>.Editor.asmdef.meta
    Trigger2to3.<Pkg>.Editor.asset               ← UdonSharpAssemblyDefinition
    Trigger2to3.<Pkg>.Editor.asset.meta

Runtime/ProgramAsset/Integration/<Pkg>/
    T23_My<Pkg>Action.asset                      ← STUB, создать вручную (обязателен!)
    T23_My<Pkg>Action.asset.meta
```

### 1. Runtime asmdef

```json
{
    "name": "Trigger2to3.<Pkg>.Runtime",
    "references": [
        "Trigger2to3.Runtime",
        "VRC.SDK3",
        "VRC.SDKBase",
        "VRC.Udon",
        "UdonSharp.Runtime",
        "<PkgAssemblyName>"
    ],
    "defineConstraints": ["<PKG_DEFINE>"],
    "autoReferenced": true
}
```

> Имя сборки пакета (`<PkgAssemblyName>`) — брать из поля `"name"` в `.asmdef` файле самого пакета  
> (`Packages/<pkg-id>/Runtime/*.asmdef`), **не** по имени файла и не по имени UdonSharpAssemblyDefinition asset.  
> Пример: LTCGI → `LTCGI_Assembly.asmdef` → `"name": "LTCGI_Assembly"`.

### 2. Editor asmdef

```json
{
    "name": "Trigger2to3.<Pkg>.Editor",
    "references": [
        "Trigger2to3.Runtime",
        "Trigger2to3.Editor",
        "Trigger2to3.<Pkg>.Runtime",
        "VRC.SDK3",
        "VRC.SDKBase",
        "VRC.Udon",
        "UdonSharp.Runtime",
        "UdonSharp.Editor",
        "<PkgAssemblyName>"
    ],
    "defineConstraints": ["<PKG_DEFINE>"],
    "includePlatforms": ["Editor"],
    "autoReferenced": true
}
```

### 3. UdonSharpAssemblyDefinition asset

Создать вручную рядом с каждым `.asmdef`. Скопировать формат из `Runtime/Trigger2to3.asset`, заменить только GUID `sourceAssembly` на GUID из `.asmdef.meta`:

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 5136146375e9a0a498a72a0091b40cc1, type: 3}
  m_Name: Trigger2to3.<Pkg>.Runtime
  m_EditorClassIdentifier:
  sourceAssembly: {fileID: 11500000, guid: <GUID_FROM_ASMDEF_META>, type: 3}
```

### 4. Runtime script (класс)

```csharp
#if <PKG_DEFINE>
using UdonSharp;
using UnityEngine;
// using пакетные типы

namespace Trigger2to3
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class T23_My<Pkg>Action : T23_ActionBase
    {
        // поля пакета
        public PkgAdapter adapter;

        // если используется DrawToggleOperationField() / DrawBoolOperationField():
        public bool toggle;
        public bool operation = true;
        public T23_PropertyBox propertyBox;
        public bool usePropertyBox;

        protected override void PostStart()
        {
            // инициализация (не OnStart — базовый OnStart регистрирует в broadcast)
        }

        protected override void OnAction()
        {
            if (usePropertyBox && propertyBox)
                operation = propertyBox.value_b;

            // логика
        }
    }
}
#endif
```

### 5. Editor script

```csharp
#if <PKG_DEFINE> && UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UnityEngine;

namespace Trigger2to3
{
    [CustomEditor(typeof(T23_My<Pkg>Action))]
    internal class T23_My<Pkg>ActionEditor : T23_ActionEditorBase
    {
        protected override void DrawFields()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("adapter"));
            DrawToggleOperationField();  // только если класс имеет propertyBox/usePropertyBox
            // DrawRecieversList("recievers");
        }
    }
}
#endif
```

### 6. Program asset stub (ОБЯЗАТЕЛЕН)

VPM пакеты устанавливаются в `Packages/<id>/` — директория writable, но **UdonSharp не создаёт новые `.asset` файлы автоматически**. Без stub — компонент не работает ("Unable to find valid U# program asset").

Создать `Runtime/ProgramAsset/Integration/<Pkg>/T23_My<Pkg>Action.asset`:

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: c333ccfdd0cbdbc4ca30cef2dd6e6b9b, type: 3}
  m_Name: T23_My<Pkg>Action
  m_EditorClassIdentifier:
  serializedUdonProgramAsset: {fileID: 0}
  udonAssembly:
  assemblyError:
  sourceCsScript: {fileID: 11500000, guid: <GUID_FROM_CS_META>, type: 3}
  scriptVersion: 2
  compiledVersion: 0
  behaviourSyncMode: 0
  hasInteractEvent: 0
  scriptID: 0
  serializationData:
    SerializedFormat: 2
    SerializedBytes:
    ReferencedUnityObjects: []
    SerializedBytesString:
    Prefab: {fileID: 0}
    PrefabModificationsReferencedUnityObjects: []
    PrefabModifications: []
    SerializationNodes: []
```

`c333ccfdd0cbdbc4ca30cef2dd6e6b9b` — GUID типа `UdonSharpProgramAsset` (константа).  
`<GUID_FROM_CS_META>` — GUID из `.cs.meta` файла runtime класса.

> `_gen_meta_assets.py` пропускает integration скрипты (из-за `defineConstraints`). После создания stub вручную — запустить скрипт чтобы сгенерировать `.meta` для нового `.asset`.

### 7. Категория в меню

Добавить в `T23_EditorUtility.cs` → `s_ActionCategories`:

```csharp
{ "T23_My<Pkg>Action", "Integration" },
```

`TypeCache.GetTypesDerivedFrom` уже сканирует все сборки — тип найдётся автоматически, нужна только запись категории.

### Чеклист — интеграционный модуль

- [ ] `Runtime/Script/Integration/<Pkg>/T23_My<Pkg>Action.cs` — весь файл в `#if <PKG_DEFINE>`
- [ ] `Runtime/Script/Integration/<Pkg>/Trigger2to3.<Pkg>.Runtime.asmdef` — `defineConstraints`, правильный `name` сборки пакета (проверить в `.asmdef` пакета)
- [ ] `Runtime/Script/Integration/<Pkg>/Trigger2to3.<Pkg>.Runtime.asset` — UdonSharpAssemblyDefinition с правильным `sourceAssembly` GUID
- [ ] `Editor/Integration/<Pkg>/T23_My<Pkg>ActionEditor.cs` — гвард `#if <PKG_DEFINE> && UNITY_EDITOR && !COMPILER_UDONSHARP`
- [ ] `Editor/Integration/<Pkg>/Trigger2to3.<Pkg>.Editor.asmdef` + `.asset`
- [ ] `Runtime/ProgramAsset/Integration/<Pkg>/T23_My<Pkg>Action.asset` — **stub вручную**, `sourceCsScript` GUID из `.cs.meta`
- [ ] `_gen_meta_assets.py` для `.meta` stub asset
- [ ] Категория `"Integration"` добавлена в `T23_EditorUtility.cs`
- [ ] Если Editor использует `DrawToggleOperationField()` — в runtime классе есть `propertyBox` + `usePropertyBox`
- [ ] Компиляция в Unity с установленным пакетом — без ошибок
- [ ] Компиляция без пакета (удалить из VCC) — без ошибок

---

## 4. Генерация .asset

```powershell
python _gen_meta_assets.py
```

Создаст `Runtime/ProgramAsset/<тип>/T23_MyNew.asset` + `.meta` для всех новых скриптов.  
Запускать из корня репо (`Puru_T23/`).

> **Важно:** скрипты, обёрнутые в `#if UNITY_EDITOR`, пропускаются автоматически — `.asset` для них не создаётся.

---

## 5. Релиз

1. Открыть Unity → дождаться компиляции → проверить в сцене
2. Бамп версии в `package.json`
3. Запись в `CHANGELOG.md`
4. `python _validate_release.py` → должно быть OK
5. Коммит + push main
6. Тег **после** push: `git tag vX.X.X-fork.Y && git push origin vX.X.X-fork.Y`

---

## Чеклист

- [ ] `Runtime/Script/<тип>/T23_MyNew.cs` — наследует `T23_TriggerBase` или `T23_ActionBase`
- [ ] `Editor/<тип>/T23_MyNewEditor.cs` — `#if UNITY_EDITOR`, `[CustomEditor(typeof(...))]`
- [ ] Категория добавлена в `T23_EditorUtility.cs`
- [ ] `python _gen_meta_assets.py` запущен, `.asset` создан
- [ ] Компиляция в Unity — без ошибок
- [ ] `package.json` + `CHANGELOG.md` обновлены
- [ ] `python _validate_release.py` — OK
