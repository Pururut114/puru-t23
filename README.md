# Puru T23 — Personal Fork of Trigger2to3

Event-driven Trigger/Broadcast/Action система для VRChat миров.  
Личный форк [Trigger2to3 by Hoke](https://github.com/hoke946/Trigger2to3_VPM) (MIT).

**Не аффилирован с оригинальным автором. Для приватного использования.**

---

## Установка

1. Добавить репо в VCC: `vcc://vpm/add-repo?url=https://Pururut114.github.io/puru-t23/index.json`
2. Установить **Puru T23** через VCC
3. Готово — компоненты доступны через `Add Component → T23 / ...`

---

## Что это

43 Trigger + 62 Action + 2 Broadcast = готовая система событий без кода.

**Базовый флоу:**
```
[Trigger] → [Broadcast] → [Action(s)]
```
Все компоненты группы вешаются на один GameObject и связываются через `groupID`.

Подробнее — [GUIDE.md](GUIDE.md): концепция, примеры, полный справочник модулей.

---

## Изменения относительно оригинала (v2.2.3)

| Что | Как изменено |
|-----|-------------|
| `T23_UseLegacyLocomotion` | API убран из VRChat SDK → заменён на `Debug.LogWarning` |
| `T23_PropertyBox` | Убран блок `trackType == 6` с `Input.GetAxis` (не в Udon whitelist) |
| `T23_PickupHaptic` | `VRC_Pickup.PlayHaptics()` заменён на `VRCPlayerApi.PlayHapticEventInHand()`, добавлены поля duration/amplitude/frequency |
| Editor UI | Inspector заголовки: цветные плашки, group-aware. Все строки локализованы |
| Add-меню Master | Семантические категории вместо первых букв |
| Дублирующиеся assets | 5 дублей `* Udon.asset` из upstream удалены |
| `T23_SetLtcgiState` | Новый Action: вкл/выкл LTCGI глобально или по отдельным экранам (опциональная зависимость) |

---

## Структура репо

```
Runtime/Script/      — UdonSharp компоненты
  Trigger/           — 43 триггера
  Action/            — 62 экшена
  Broadcast/         — Local + Global
  Base/              — базовые классы
  Option/            — PropertyBox, AudioBank, CommonBuffer, ConnectFromUdon
  Integration/       — модули с опциональными зависимостями (LTCGI...)

Editor/              — Custom Inspector'ы для каждого компонента
Runtime/ProgramAsset/— UdonSharp program assets (генерируются / включены в репо)
Docs/                — ADDING_MODULES.md (гайд по добавлению новых модулей)
```

---

## Версионирование

Схема: `<upstream>-fork.<N>` → `2.2.3-fork.12`, `2.2.3-fork.13`...  
При выходе нового апстрима: `2.2.4-fork.1`.

---

## Лицензия

MIT. Оригинальный copyright Hoke сохранён — см. [LICENSE](LICENSE).
