# Puru T23 — Руководство по использованию

Гайд по работе с системой Trigger/Broadcast/Action для VRChat миров.  
Основан на [Trigger2to3 by Hoke](https://github.com/hoke946/Trigger2to3_VPM).

---

## Концепция

T23 — event-driven система для VRChat: **что-то произошло → что-то делаем**.  
Все компоненты вешаются на один и тот же GameObject.

```
[Trigger] → [Broadcast] → [Action(s)]
```

- **Trigger** — детектирует событие (игрок нажал, вошёл в зону, прошёл таймер...)
- **Broadcast** — передаёт событие Actions (локально или по сети)
- **Action** — выполняет действие (включить объект, запустить анимацию, телепортировать...)
- **Master** — необязателен, но позволяет визуально управлять несколькими группами на одном GameObject

Все компоненты одной группы связаны через **groupID** (int). Trigger с groupID=0 активирует все Broadcast и Action с groupID=0 на этом объекте.

---

## Быстрый старт

### 1. Создать группу

1. Создать GameObject в сцене
2. `Add Component → T23 / Master` (опционально, для удобства)
3. Добавить нужный Trigger через кнопку **Trigger Interact** (или вручную через Add Component)
4. Добавить Broadcast (Local или Global)
5. Добавить Action

Если Master добавлен — groupID, приоритет и порядок управляются из него. Без Master — каждый компонент настраивается отдельно.

### 2. Минимальная настройка (без Master)

```
GameObject
  ├── T23_OnInteract    groupID=0
  ├── T23_BroadcastLocal    groupID=0
  └── T23_SetGameObjectActive    groupID=0, recievers=[Door], operation=true
```

---

## Практические примеры

### Кнопка открывает дверь

**Задача:** игрок нажимает кнопку → дверь становится активной.

```
Button GameObject
  ├── T23_OnInteract          groupID=0, Interaction Text="Открыть"
  ├── T23_BroadcastLocal      groupID=0
  └── T23_SetGameObjectActive groupID=0, recievers=[Door], operation=true
```

Чтобы кнопка переключала (открыть/закрыть) — поставить **toggle=true** в Action.

---

### Триггер зона (вошёл → звук)

**Задача:** игрок входит в зону → играет звук.

```
Zone GameObject (с Collider, Is Trigger=true)
  ├── T23_OnEnterTrigger      groupID=0, localOnly=true
  ├── T23_BroadcastLocal      groupID=0
  └── T23_AudioPlay           groupID=0, recievers=[AudioSource]
```

`localOnly=true` — событие только для того игрока, кто вошёл.

---

### Таймер с рандомным периодом

**Задача:** каждые 3–7 секунд активировать случайный эффект.

```
Timer GameObject
  ├── T23_OnTimer             groupID=0, repeat=true, lowPeriodTime=3, highPeriodTime=7
  ├── T23_BroadcastLocal      groupID=0, randomize=true
  └── T23_SetParticlePlaying  groupID=0, recievers=[FX1], randomAvg=0.33
  └── T23_SetParticlePlaying  groupID=0, recievers=[FX2], randomAvg=0.33
  └── T23_SetParticlePlaying  groupID=0, recievers=[FX3], randomAvg=0.34
```

`randomize` на Broadcast + `randomAvg` на каждом Action = рандомный выбор одного из нескольких.

---

### Сетевая синхронизация (BroadcastGlobal)

**Задача:** один игрок нажал кнопку → все видят эффект.

```
Button GameObject
  ├── T23_OnInteract          groupID=0
  ├── T23_BroadcastGlobal     groupID=0, sendTarget=All
  └── T23_SetGameObjectActive groupID=0, recievers=[SharedEffect], operation=true
```

`sendTarget`:
- `All` — все игроки, включая нажавшего
- `Others` — все кроме нажавшего
- `Owner` — только владелец объекта

`bufferType` на BroadcastGlobal:
- `0` — без буферизации (новые игроки не получат событие)
- `1` — буферизовать последнее (новые игроки догоняют состояние через CommonBuffer)

---

### Условная логика (PropertyBox + ConditionalTrigger)

**Задача:** открыть дверь только если счётчик >= 3.

**Шаг 1 — Счётчик:**
```
Counter GameObject
  ├── T23_OnInteract          groupID=0
  ├── T23_BroadcastLocal      groupID=0
  └── T23_SetPropertyBox      groupID=0, propertyBox=[CounterBox], calcOperator=Add, value_int=1
```

**Шаг 2 — Проверка:**
```
Check GameObject
  ├── T23_ConditionalTrigger  groupID=0, basePropertyBox=[CounterBox],
  │                           compOperator=GreaterEqual, compParameterType=Const, value_int=3
  ├── T23_BroadcastLocal      groupID=0
  └── T23_SetGameObjectActive groupID=0, recievers=[Door], operation=true
```

Активировать проверку из шага 1: добавить `T23_ActiveConditionalTrigger` с `recievers=[Check GameObject]`.

---

### AudioBank — плейлист

**Задача:** кнопка переключает треки по порядку.

```
MusicPlayer GameObject
  ├── T23_AudioBank           source=[AudioSource], playbackOrder=Sequential,
  │                           clips=[Track1, Track2, Track3]
  ├── T23_OnInteract          groupID=0
  ├── T23_BroadcastLocal      groupID=0
  └── T23_UseAudioBank        groupID=0, audioBank=[MusicPlayer], command=Next
```

Команды AudioBank: `Play=0`, `Stop=1`, `Next=2`, `Previous=3`, `Shuffle=4`.

---

### Активация кастомного триггера из Udon

**Задача:** вызвать T23 действие из своего UdonSharp скрипта.

```
T23 GameObject
  ├── T23_CustomTrigger       groupID=0, Name="OpenDoor"
  ├── T23_BroadcastLocal      groupID=0
  └── T23_SetGameObjectActive groupID=0, recievers=[Door], operation=true
```

В Udon скрипте:
```csharp
// Вариант 1: ConnectFromUdon компонент
// connectFromUdon.trigger = "OpenDoor"
// SendCustomEvent на ConnectFromUdon

// Вариант 2: напрямую
var customTrigger = GetComponent<T23_CustomTrigger>();
// customTrigger.Trigger() через reflection или SendCustomEvent
```

Или добавить `T23_ConnectFromUdon` с `customTriggerName="OpenDoor"` и вызывать `SendCustomEvent("Trigger")` на нём.

---

## PropertyBox

Контейнер значения с возможностью отслеживать данные объекта/игрока в реальном времени.

**Типы значений (valueType):**
- `0` — bool
- `1` — int
- `2` — float
- `3` — Vector3
- `4` — string

**Режимы отслеживания (trackType):**
- `0` — статичное значение (константа)
- `1` — позиция GameObject
- `2` — активность GameObject
- `3` — параметр Animator
- `4` — значение UI компонента (Toggle/Slider/Dropdown)
- `5` — данные игрока (позиция, скорость, рост...)
- `7` — системное время

`updateEveryFrame=true` — обновлять каждый кадр (нужно для быстро меняющихся данных).

---

## Справочник модулей

### Broadcasts

| Компонент | Описание |
|-----------|----------|
| `T23_BroadcastLocal` | Локальный — выполняет Actions только у текущего клиента. |
| `T23_BroadcastGlobal` | Сетевой — отправляет событие всем (All) или другим (Others) или владельцу (Owner). Поддерживает буферизацию через `T23_CommonBuffer`. |

---

### Triggers

#### Lifecycle
| Компонент | Событие |
|-----------|---------|
| `T23_OnSpawn` | Объект заспаунен (networked) |
| `T23_OnEnable` | GameObject активирован (следующий кадр) |
| `T23_OnDisable` | GameObject деактивирован |
| `T23_OnDestroy` | GameObject уничтожен |
| `T23_OnNetworkReady` | Сеть готова к синхронизации |

#### Interaction
| Компонент | Событие | Ключевые поля |
|-----------|---------|---------------|
| `T23_OnInteract` | Игрок взаимодействует с объектом | Interaction Text, Proximity |
| `T23_OnPickup` | Подобрал VRC_Pickup | |
| `T23_OnDrop` | Бросил VRC_Pickup | |
| `T23_OnPickupUseDown` | Зажал кнопку Use на пикапе | |
| `T23_OnPickupUseUp` | Отпустил кнопку Use на пикапе | |
| `T23_OnEnterTrigger` | Вошёл в Trigger Collider | `localOnly`, `layers` |
| `T23_OnExitTrigger` | Вышел из Trigger Collider | `localOnly`, `layers` |
| `T23_OnEnterCollider` | Столкновение (OnCollisionEnter) | `localOnly`, `layers` |
| `T23_OnExitCollider` | Конец столкновения (OnCollisionExit) | `localOnly`, `layers` |
| `T23_OnParticleCollision` | Частица столкнулась | `layers` |
| `T23_OnStationEntered` | Вошёл в станцию | `localOnly` |
| `T23_OnStationExited` | Вышел из станции | `localOnly` |

#### Input
| Компонент | Событие | Ключевые поля |
|-----------|---------|---------------|
| `T23_OnKeyDown` | Клавиша нажата | `key` (KeyCode), `keyFree` (строка) |
| `T23_OnKeyUp` | Клавиша отпущена | `key`, `keyFree` |
| `T23_InputJump` | Прыжок | `inputValue` (down/up) |
| `T23_InputGrab` | Grab кнопка | `inputValue`, `hand` |
| `T23_InputDrop` | Drop кнопка | `inputValue`, `hand` |
| `T23_InputUse` | Use кнопка | `inputValue`, `hand` |

#### Player
| Компонент | Событие | Ключевые поля |
|-----------|---------|---------------|
| `T23_OnPlayerJoined` | Игрок зашёл | `excludeLocal`, `excludePostJoining` |
| `T23_OnPlayerLeft` | Игрок ушёл | `excludeLocal` |
| `T23_OnPlayerRespawn` | Игрок респаунился | `localOnly` |
| `T23_OnAvatarChanged` | Сменил аватар | `localOnly` |
| `T23_OnAvatarEyeHeightChanged` | Изменился рост аватара | `localOnly` |

#### Network
| Компонент | Событие | Ключевые поля |
|-----------|---------|---------------|
| `T23_OnOwnershipTransfer` | Сменился владелец объекта | `localOnly` |

#### Timer
| Компонент | Событие | Ключевые поля |
|-----------|---------|---------------|
| `T23_OnTimer` | Таймер сработал | `repeat`, `resetOnEnable`, `lowPeriodTime`, `highPeriodTime` |

#### Video
| Компонент | Событие |
|-----------|---------|
| `T23_OnVideoStart` | Видео началось |
| `T23_OnVideoPlay` | Видео воспроизводится |
| `T23_OnVideoPause` | Видео на паузе |
| `T23_OnVideoEnd` | Видео закончилось |

#### UI
| Компонент | Событие | Ключевые поля |
|-----------|---------|---------------|
| `T23_UIOnClickButton` | Нажата кнопка UI | `button` |
| `T23_UIOnEndEdit` | Завершено редактирование InputField | `inputField` |
| `T23_UIOnValueChanged` | Изменено значение Toggle/Dropdown | `UIComponent`, `any`, `isOn`, `value` |

#### MIDI
| Компонент | Событие | Ключевые поля |
|-----------|---------|---------------|
| `T23_MidiNoteOn` | MIDI Note On | `channel`, `note` |
| `T23_MidiNoteOff` | MIDI Note Off | `channel`, `note` |
| `T23_MidiControlChange` | MIDI CC | `channel`, `number` |

#### Logic
| Компонент | Описание | Ключевые поля |
|-----------|----------|---------------|
| `T23_CustomTrigger` | Именованный триггер, активируется по имени | `Name` |
| `T23_ConditionalTrigger` | Проверяет значение PropertyBox по условию | `basePropertyBox`, `compOperator`, `passive`, `allowContinuity` |

---

### Actions

#### GameObject
| Компонент | Что делает | Ключевые поля |
|-----------|------------|---------------|
| `T23_SetGameObjectActive` | Вкл/выкл/переключить GameObject | `recievers[]`, `toggle`, `operation`, `propertyBox` |
| `T23_SetChildrenActive` | Вкл/выкл все дочерние объекты | `recievers[]`, `operation` |
| `T23_SetNextChildActive` | Активировать следующий неактивный child | `recievers[]`, `operation` |
| `T23_SetRandomChildActive` | Активировать случайный неактивный child | `recievers[]` |
| `T23_SetLayer` | Сменить layer объекта | `recievers[]`, `layer` |
| `T23_SetParent` | Сменить родителя объекта | `recievers[]`, `parent`, `worldPositionStays` |
| `T23_DestroyObject` | Уничтожить объект | `recievers[]` |
| `T23_TeleportObject` | Телепортировать объект | `recievers[]`, `teleportLocation` / `byValue` |
| `T23_TakeOwnership` | Взять ownership | `recievers[]` |

#### Renderer / Material
| Компонент | Что делает | Ключевые поля |
|-----------|------------|---------------|
| `T23_SetRendererActive` | Вкл/выкл Renderer | `recievers[]`, `toggle`, `operation` |
| `T23_SetMaterial` | Сменить материал | `recievers[]`, `material` |

#### Animation
| Компонент | Что делает | Ключевые поля |
|-----------|------------|---------------|
| `T23_SetAnimatorActive` | Вкл/выкл Animator компонент | `recievers[]`, `toggle`, `operation` |
| `T23_AnimationBool` | Установить bool параметр | `recievers[]`, `variable`, `toggle`, `operation` |
| `T23_AnimationFloat` | Установить float параметр | `recievers[]`, `variable`, `operation` |
| `T23_AnimationInt` | Установить int параметр | `recievers[]`, `variable`, `operation` |
| `T23_AnimationIntAdd` | Прибавить к int параметру | `recievers[]`, `variable`, `operation` |
| `T23_AnimationIntSubtract` | Вычесть из int параметра | `recievers[]`, `variable`, `operation` |
| `T23_AnimationIntMultiply` | Умножить int параметр | `recievers[]`, `variable`, `operation` |
| `T23_AnimationIntDivide` | Разделить int параметр | `recievers[]`, `variable`, `operation` |
| `T23_AnimationTrigger` | Установить Trigger параметр | `recievers[]`, `trigger` |
| `T23_SetParticlePlaying` | Вкл/выкл/переключить ParticleSystem | `recievers[]`, `toggle`, `operation` |

#### Physics
| Компонент | Что делает | Ключевые поля |
|-----------|------------|---------------|
| `T23_SetIsKinematic` | Кинематика Rigidbody | `recievers[]`, `operation`, `takeOwnership` |
| `T23_SetUseGravity` | Гравитация Rigidbody | `recievers[]`, `operation`, `takeOwnership` |
| `T23_SetVelocity` | Задать скорость Rigidbody | `recievers[]`, `velocity`, `useWorldSpace` |
| `T23_AddVelocity` | Добавить скорость Rigidbody | `recievers[]`, `velocity`, `useWorldSpace` |
| `T23_SetAngularVelocity` | Задать угловую скорость | `recievers[]`, `angularVelocity` |
| `T23_AddAngularVelocity` | Добавить угловую скорость | `recievers[]`, `angularVelocity` |
| `T23_AddForce` | Приложить силу | `recievers[]`, `force`, `useWorldSpace` |

#### Audio
| Компонент | Что делает | Ключевые поля |
|-----------|------------|---------------|
| `T23_AudioPlay` | Вкл/выкл/переключить AudioSource | `recievers[]`, `toggle`, `operation` |
| `T23_AudioPause` | Пауза AudioSource | `recievers[]`, `operation` |
| `T23_AudioTrigger` | PlayOneShot | `recievers[]` |
| `T23_UseAudioBank` | Управление AudioBank | `audioBank`, `command` (Play/Stop/Next/Prev/Shuffle), `index` |

#### Player
| Компонент | Что делает | Ключевые поля |
|-----------|------------|---------------|
| `T23_TeleportPlayer` | Телепортировать игрока | `teleportLocation` / `byValue`, `teleportOrientation`, `lerpOnRemote` |
| `T23_SetPlayerSpeed` | Скорость ходьбы/бега/стрейфа | `walkSpeed`, `runSpeed`, `strafeSpeed` |
| `T23_SetPlayerVelocity` | Задать скорость игрока | `velocity` |
| `T23_AddPlayerVelocity` | Добавить скорость игрока | `velocity` |
| `T23_SetJumpImpulse` | Высота прыжка | `impulse` |
| `T23_SetGravityStrength` | Сила гравитации игрока | `gravityStrength` |
| `T23_SetAvatarEyeHeight` | Рост аватара | `scaleMeters` |
| `T23_SetAvatarUse` | Посадить на пьедестал | |
| `T23_SetVoiceParameters` | Параметры голоса | `distanceFar`, `gain`, `lowpass`, `volumetricRadius` |
| `T23_SetAvatarAudioParameters` | Параметры аватарного аудио | `gain`, `farRadius`, `nearRadius` |

#### Teleport
| Компонент | Что делает | Ключевые поля |
|-----------|------------|---------------|
| `T23_TeleportPlayer` | Телепорт игрока | `teleportLocation`, `lerpOnRemote` |
| `T23_TeleportObject` | Телепорт объекта | `recievers[]`, `teleportLocation`, `removeVelocity` |

#### Pickup
| Компонент | Что делает | Ключевые поля |
|-----------|------------|---------------|
| `T23_PickupDrop` | Бросить пикап | `recievers[]` |
| `T23_PickupHaptic` | Haptic feedback в руке | `recievers[]`, `duration`, `amplitude`, `frequency` |

#### UI
| Компонент | Что делает | Ключевые поля |
|-----------|------------|---------------|
| `T23_SetUIText` | Установить текст | `recievers[]`, `text`, `propertyBox` |
| `T23_SetUIBool` | Установить Toggle | `recievers[]`, `toggle`, `operation`, `withoutNotify` |
| `T23_SetUIFloat` | Установить Slider/Scrollbar | `recievers[]`, `operation`, `withoutNotify` |
| `T23_SetUIInt` | Установить Dropdown | `recievers[]`, `operation`, `withoutNotify` |

#### Object Pool
| Компонент | Что делает | Ключевые поля |
|-----------|------------|---------------|
| `T23_SpawnObjectPool` | Заспаунить объект из пула | `objectPool` |
| `T23_ReturnObjectPool` | Вернуть объект в пул | `objectPool` |
| `T23_ReturnObjectPoolAll` | Вернуть все объекты в пул | `objectPool` |
| `T23_ShuffleObjectPool` | Перемешать пул | `objectPool` |
| `T23_SpawnObject` | Instantiate prefab (client-only) | `prefab`, `locations[]` |

#### System / Logic
| Компонент | Что делает | Ключевые поля |
|-----------|------------|---------------|
| `T23_SetPropertyBox` | Записать/изменить значение в PropertyBox | `propertyBox`, `calcOperator` (Set/Add/Sub/Mul/Div), `value_*` |
| `T23_ActiveConditionalTrigger` | Запустить проверку ConditionalTrigger | `recievers[]` |
| `T23_ActiveCustomTrigger` | Активировать CustomTrigger по имени | `recievers[]`, `Name` |
| `T23_CallUdonMethod` | Вызвать custom event на UdonBehaviour | `udonBehaviour`, `method`, `ownershipControl` |

#### Integration
| Компонент | Что делает | Ключевые поля |
|-----------|------------|---------------|
| `T23_SetLtcgiState` | Вкл/выкл LTCGI глобально или по экранам | `adapter`, `global`, `toggle`, `operation`, `screens[]` |

#### Deprecated
| Компонент | Статус |
|-----------|--------|
| `T23_UseLegacyLocomotion` | Заглушка — API удалён из VRChat SDK, выводит Warning |

---

### Options (вспомогательные компоненты)

| Компонент | Назначение |
|-----------|------------|
| `T23_PropertyBox` | Контейнер значения (bool/int/float/Vector3/string) с трекингом объектов, игроков, UI, аниматора, времени |
| `T23_AudioBank` | Плейлист аудио: Sequential/Random, pitch randomization, callbacks при play/stop/change |
| `T23_CommonBuffer` | Буфер синхронизации для BroadcastGlobal — новые игроки догоняют состояние |
| `T23_ConnectFromUdon` | Мост для вызова CustomTrigger из внешнего UdonSharp скрипта |

---

## Полезные паттерны

### One-shot (сработать один раз)

```
Button
  ├── T23_OnInteract
  ├── T23_BroadcastLocal
  ├── T23_SetGameObjectActive   recievers=[Button], operation=false  ← выключить себя
  └── T23_SetGameObjectActive   recievers=[Effect], operation=true
```

### Сетевое состояние с буферизацией (новые игроки видят актуальное)

```
NetworkedObject  (добавить UdonBehaviour с ownership)
  ├── T23_CommonBuffer          broadcasts=[MyBroadcast]
  ├── T23_OnInteract
  ├── T23_BroadcastGlobal       groupID=0, sendTarget=All, bufferType=1, commonBuffer=[CommonBuffer]
  └── T23_SetGameObjectActive   groupID=0, recievers=[SharedObj], toggle=true
```

### Последовательная активация (слайдшоу)

```
Controller
  ├── T23_OnInteract
  ├── T23_BroadcastLocal
  └── T23_SetNextChildActive    recievers=[SlideContainer]
```

### Случайный выбор из нескольких Actions

```
Controller
  ├── T23_OnInteract
  ├── T23_BroadcastLocal        randomize=true
  ├── T23_AudioPlay             recievers=[Bark1], randomAvg=0.33
  ├── T23_AudioPlay             recievers=[Bark2], randomAvg=0.33
  └── T23_AudioPlay             recievers=[Bark3], randomAvg=0.34
```

---

## Ограничения

- `T23_BroadcastGlobal` — max 10 групп на одном GameObject (hardcoded RecieveNetworkFire0–9)
- `T23_SpawnObject` — client-side `Instantiate`, без сетевой синхронизации
- UI компоненты — только legacy `UnityEngine.UI` (Text, InputField, Dropdown); TextMeshPro не поддерживается
- `T23_UseLegacyLocomotion` — заглушка, ничего не делает
