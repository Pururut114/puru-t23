# Puru T23 — Оперативные заметки

## Репо

- **GitHub:** https://github.com/Pururut114/puru-t23
- **VPM listing:** https://Pururut114.github.io/puru-t23/index.json
- **VCC install URL:** `vcc://vpm/add-repo?url=https://Pururut114.github.io/puru-t23/index.json`
- **Package ID:** `com.pururut.t23`
- **Оригинал:** https://github.com/hoke946/Trigger2to3_VPM (MIT, by Hoke)

---

## Рабочий процесс — новый релиз

```powershell
# 1. Обновить version в package.json ПЕРВЫМ
# 2. Обновить CHANGELOG.md (создать если нет)
git add .
git commit -m "release: Puru T23 vX.X.X-fork.Y"
git push origin main
git tag vX.X.X-fork.Y
git push origin vX.X.X-fork.Y
# → release.yml создаёт zip + Release
# → build-listing.yml обновляет index.json на gh-pages
```

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
