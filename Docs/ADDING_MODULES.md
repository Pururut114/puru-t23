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

        // Дополнительные поля:
        // public bool toggle;
        // public float value;

        protected override void OnAction()
        {
            for (int i = 0; i < recievers.Length; i++)
            {
                if (recievers[i]) Execute(recievers[i]);
            }
        }

        private void Execute(SomeType target)
        {
            // логика
        }
    }
}
```

`randomAvg`, `priority`, `groupID`, `title` — уже есть в базовых классах, не добавлять повторно.

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
