# Puru T23 — Оперативные заметки

## Инструменты

| Скрипт | Назначение |
|--------|-----------|
| `_gen_meta_assets.py` | Генерирует .meta для всех файлов + .asset для новых UdonSharpBehaviour. Запускать после добавления нового скрипта. |
| `_validate_release.py` | Pre-tag валидатор: version vs CHANGELOG, тег не существует, все .asset и .meta на месте. |

### Добавить новый Trigger/Action
1. Написать `.cs` в `Runtime/Script/<тип>/T23_MyNew.cs`
2. Написать Editor Inspector в `Editor/<тип>/T23_MyNewEditor.cs`
3. `python _gen_meta_assets.py` → создаст `Runtime/ProgramAsset/<тип>/T23_MyNew.asset` + все .meta
4. Открыть Unity → дождаться компиляции → убедиться что всё работает
5. Бамп версии в `package.json`, запись в `CHANGELOG.md`
6. `python _validate_release.py` → должно быть OK
7. Релиз (см. ниже)

## Следующие шаги

### ~~Step 1 — SDK/Udon валидация~~ ✓ ВЫПОЛНЕНО
Аудит проведён. Все 80+ скриптов проверены. Найдено и исправлено:
- `T23_PickupHaptic`: `VRC_Pickup.PlayHaptics()` → `VRCPlayerApi.PlayHapticEventInHand()` (fork.3)
- Дублирующиеся `* Udon.asset` из upstream + orphan `T23_UIOnValueChangedBool.asset` → удалены (fork.4)
- `Input.GetKeyDown/GetKeyUp(KeyCode)` — OK, в whitelist
- `VRCObjectPool.Pool` + foreach — OK

### ~~Step 2 — Кастомные модули~~ ✓ ВЫПОЛНЕНО
Гайд написан: `Docs/ADDING_MODULES.md`
Первый модуль реализован: `T23_SetLtcgiState` (LTCGI global/per-screen toggle, fork.6→fork.12)

**Паттерн для интеграций с опциональной зависимостью (финальный):**
1. Отдельный asmdef в `Runtime/Script/Integration/<Pkg>/` с `defineConstraints: ["PKG_DEFINE"]` и правильным assembly reference
2. `UdonSharpAssemblyDefinition` `.asset` рядом с asmdef (создаётся вручную, формат из `Trigger2to3.asset`)
3. Класс в `.cs` оборачивать в `#if PKG_DEFINE ... #endif`; Editor `.cs` — `#if PKG_DEFINE && UNITY_EDITOR && !COMPILER_UDONSHARP`
4. **Program asset stub ВКЛЮЧАТЬ в репо** (`Runtime/ProgramAsset/Integration/<Pkg>/`) — VPM пакеты writable, но UdonSharp новые `.asset` сам не создаёт. Формат: minimal YAML с `sourceCsScript guid` из `.cs.meta`
5. `_gen_meta_assets.py` и `_validate_release.py` пропускают `defineConstraints`-скрипты автоматически (для stub это OK — файл уже есть)
6. `GetModuleClasses` использует `TypeCache.GetTypesDerivedFrom` — сканирует все сборки, находит типы из отдельных assembly
7. **Action с toggle/operation UI обязан объявить `propertyBox` (T23_PropertyBox) и `usePropertyBox` (bool)** — `DrawToggleOperationField()` / `DrawBoolOperationField()` вызывают `FindProperty` по этим именам; если полей нет → NullReferenceException в инспекторе. В `OnAction()`: `if (usePropertyBox && propertyBox) operation = propertyBox.value_b;`

**LTCGI-специфично:**
- Assembly reference = `"LTCGI_Assembly"` (имя из поля `name` в `LTCGI_Assembly.asmdef`)
- `"LTCGI_AssemblyUdon"` — название UdonSharpAssemblyDefinition `.asset`, НЕ имя сборки
- Пакет: `at.pimaker.ltcgi`, define: `LTCGI_INCLUDED`

### ~~Step 3 — Тест новых модулей (fork.13)~~ ✓ ВЫПОЛНЕНО

`T23_SetPickupable` и `T23_SetAvatarScaling` протестированы, работают.

### Misc
- Протестировать VCC install: `vcc://vpm/add-repo?url=https://Pururut114.github.io/puru-t23/index.json`
- ~~Импорт в реальный Unity проект, проверить компиляцию~~ ✓ fork.12 — LTCGI интеграция работает

## Репо

- **GitHub:** https://github.com/Pururut114/puru-t23
- **VPM listing:** https://Pururut114.github.io/puru-t23/index.json
- **VCC install URL:** `vcc://vpm/add-repo?url=https://Pururut114.github.io/puru-t23/index.json`
- **Package ID:** `com.pururut.t23`
- **Оригинал:** https://github.com/hoke946/Trigger2to3_VPM (MIT, by Hoke)

---

## Рабочий процесс — новый релиз

```powershell
# 1. Убедиться что ВСЕ изменения закоммичены и запушены в main
# 2. Обновить version в package.json
# 3. Обновить CHANGELOG.md
git add .
git commit -m "release: Puru T23 vX.X.X-fork.Y"
git push origin main
git tag vX.X.X-fork.Y
git push origin vX.X.X-fork.Y
# → release.yml создаёт zip + Release
# → build-listing.yml обновляет index.json на gh-pages
```

> **Важно:** тег должен ставиться ПОСЛЕДНИМ — после того как все коммиты релиза уже в main. Тег фиксирует именно тот коммит, на котором стоит; изменения после тега в релиз не попадают.

Следить за Actions: `gh run watch`

---

## VPM инфраструктура

### GitHub Actions (аналогично PSS)

- `release.yml` — триггер на `v*` тег → zip + Release
- `build-listing.yml` — `workflow_run` после release → обновляет `gh-pages/index.json`

**GitHub Pages:** branch `gh-pages`, файл `index.json`.  
Обязательно наличие `.nojekyll` в ветке `gh-pages`.

### Если Actions не сработали — см. PSS NOTES.md
Аналогичная инфраструктура, те же ручные команды через API (заменить repo на `puru-t23`).

---

## Изменения относительно оригинала (v2.2.3)

| Файл | Изменение |
|------|-----------|
| `Runtime/Script/Action/T23_UseLegacyLocomotion.cs` | Убран вызов `UseLegacyLocomotion()` (удалён из VRChat SDK), добавлен Warning лог |
| `Runtime/Script/Option/T23_PropertyBox.cs` | Убран блок `trackType == 6` с `Input.GetAxis` (не в Udon whitelist) |
| `Editor/Action/T23_UseLegacyLocomotionEditor.cs` | Добавлен HelpBox с предупреждением |
| `Editor/Option/T23_PropertyBoxEditor.cs` | trackType 6 → HelpBox с ошибкой, UI локализован |
| Все Editor/*.cs | Локализация UI-строк на русский |
| `package.json` | ID: `com.pururut.t23`, version: `2.2.3-fork.1` |
| `LICENSE` | Добавлен Modifications copyright Pururut |
| `README.md` | Атрибуция, описание изменений |

---

## Версионирование

Схема: `<upstream_version>-fork.<N>`  
Пример: `2.2.3-fork.1`, `2.2.3-fork.2`, ...  
При выходе нового апстрима: `2.2.4-fork.1`, проверить совместимость, применить фиксы заново.

---

## Известные ограничения (от оригинала, не фиксим)

- `T23_BroadcastGlobal`: max 10 групп на один GameObject (hardcoded методы RecieveNetworkFire0–9)
- `T23_SpawnObject`: `Instantiate()` client-side only, без сети
- Legacy `UnityEngine.UI` (Text, InputField, Dropdown) — компилируется, но TMP не поддерживается
