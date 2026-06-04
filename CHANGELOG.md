# Changelog — Puru T23

## [2.2.3-fork.11] — 2026-06-04

### Fixed
- `T23_SetLtcgiState`: added `UdonSharpProgramAsset` `.asset` stub to repo — VPM packages are writable but UdonSharp does not auto-create new program assets; stub is required for the component to function (UdonSharp populates it on first compile)
- `T23_EditorUtility.GetModuleClasses`: changed assembly scan from `Assembly.GetAssembly(baseType).GetTypes()` to `TypeCache.GetTypesDerivedFrom(baseType)` — now scans ALL loaded assemblies, so integration actions from separate assemblies (LTCGI etc.) appear in the add menu

---

## [2.2.3-fork.10] — 2026-06-04

### Fixed
- `Trigger2to3.LTCGI.Runtime.asmdef`: corrected assembly reference to `"LTCGI_Assembly"` (the actual `.asmdef` name field). Previous attempts used `"LTCGI"` and `"LTCGI_AssemblyUdon"` — the latter is the name of LTCGI's `UdonSharpAssemblyDefinition` asset, not the assembly itself.

---

## [2.2.3-fork.9] — 2026-06-04

### Fixed
- `Trigger2to3.LTCGI.Runtime.asmdef`: corrected assembly reference from `"LTCGI"` to `"LTCGI_AssemblyUdon"` — the actual name of LTCGI's UdonSharp assembly. Previous name caused CS0246 (`LTCGI_UdonAdapter` not found) because the assembly reference silently failed to resolve.

---

## [2.2.3-fork.8] — 2026-06-04

### Fixed
- `T23_SetLtcgiState`: wrapped entire class in `#if LTCGI_INCLUDED` — belt-and-suspenders guard against compilation without LTCGI, in addition to existing `defineConstraints`. Fixes CS0246 in projects where `LTCGI_INCLUDED` scripting define is set but LTCGI package is absent (stale define in PlayerSettings after package removal).
- `T23_SetLtcgiStateEditor`: same — combined guard `#if LTCGI_INCLUDED && UNITY_EDITOR && !COMPILER_UDONSHARP`

---

## [2.2.3-fork.7] — 2026-06-04

### Fixed
- `T23_SetLtcgiState`: removed pre-generated `.asset` from repo — UdonSharp compiles scripts via its own Roslyn pipeline regardless of asmdef `defineConstraints`, causing CS0246 when LTCGI is not installed. Without `.asset` UdonSharp won't attempt compilation; with LTCGI present it auto-generates the asset on first compile.
- `_gen_meta_assets.py`: skip `.asset` generation for scripts in assemblies with `defineConstraints` (optional integrations)
- `_validate_release.py`: same — don't require `.asset` for conditionally compiled scripts

---

## [2.2.3-fork.6] — 2026-06-04

### Added
- `T23_SetLtcgiState` Action — enable/disable/toggle LTCGI system (Global or Per-Screen mode); optional dependency, compiles only when `LTCGI_INCLUDED` is defined
- `Editor/Integration/LTCGI/T23_SetLtcgiStateEditor` — Inspector with Mode, Operation and Screens fields
- `Runtime/Script/Integration/LTCGI/Trigger2to3.LTCGI.Runtime.asmdef` — conditional assembly with `defineConstraints: LTCGI_INCLUDED`
- `Editor/Integration/LTCGI/Trigger2to3.LTCGI.Editor.asmdef` — conditional editor assembly

### Fixed
- `_gen_meta_assets.py`: use `utf-8-sig` encoding to strip BOM — previously `#if UNITY_EDITOR` check failed for BOM-prefixed files, causing `T23_Master.asset` to regenerate despite the `fork.5` fix

---

## [2.2.3-fork.5] — 2026-06-04

### Fixed
- Removed `T23_Master.asset` — editor-only `MonoBehaviour` wrapped in `#if UNITY_EDITOR`, not an `UdonSharpBehaviour`; caused "Script with U# program asset must have UdonSharpBehaviour definition" error

### Changed
- `_gen_meta_assets.py`: skip classes whose file starts with `#if UNITY_EDITOR` to prevent regenerating the bogus asset

---

## [2.2.3-fork.4] — 2026-06-04

### Fixed
- Removed 5 duplicate `* Udon.asset` program assets inherited from upstream (T23_SetParent, T23_InputDrop, T23_InputGrab, T23_InputJump, T23_InputUse) — these caused UdonSharp "referenced by 2 program assets" errors
- Removed orphaned `T23_UIOnValueChangedBool.asset` (referenced T23_UIOnValueChanged.cs, but T23_UIOnValueChangedBool.cs does not exist)

---

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
