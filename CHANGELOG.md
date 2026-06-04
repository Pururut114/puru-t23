# Changelog — Puru T23

## [2.2.3-fork.3] — 2026-06-04

### Fixed
- `T23_PickupHaptic`: replaced removed `VRC_Pickup.PlayHaptics()` with `VRCPlayerApi.PlayHapticEventInHand(hand, duration, amplitude, frequency)`

### Changed
- `T23_PickupHaptic`: added `duration` (0.3), `amplitude` (0.8), `frequency` (0.5) public fields (range 0–1) for haptic parameters

---

## [2.2.3-fork.2] — 2026-06-04

### Changed
- Editor Inspector UI: `ShowTitle()` — solid colored header bar (22px), PSS-style pastel colors per module type (Master/Broadcast/Trigger/Action)
- Editor Inspector UI: `HeadlineStyle()` — theme-aware text color (light/dark editor skin)
- Master add-menus: Trigger and Action lists now grouped by semantic categories (Lifecycle, Interaction, Input, Player, Network, etc.) instead of first letter

### Added
- `_gen_meta_assets.py` — generates `.meta` + `.asset` (UdonSharpProgramAsset) for all scripts; auto-detects concrete T23 classes
- `_validate_release.py` — pre-tag release validator: version/CHANGELOG match, tag free, program assets present, UdonSharpAssemblyDefinition present, all `.meta` files present
- `CHANGELOG.md` — tracking changes from fork.1 onward

---

## [2.2.3-fork.1] — 2026-06-04

Base fork of Trigger2to3 v2.2.3 by Hoke (https://github.com/hoke946/Trigger2to3_VPM).

### Fixed
- `T23_UseLegacyLocomotion`: removed deprecated `UseLegacyLocomotion()` API call (removed from VRChat SDK); replaced with `Debug.LogWarning`
- `T23_PropertyBox`: removed `trackType == 6` block with `Input.GetAxis` calls (not in Udon whitelist)

### Changed
- Editor Inspector UI: all strings localized
- `T23_UseLegacyLocomotionEditor`: added HelpBox warning about removed API
- `T23_PropertyBoxEditor`: `trackType == 6` replaced with error HelpBox
- `package.json`: ID → `com.pururut.t23`, author → Pururut
- `LICENSE`: added Modifications copyright Pururut
