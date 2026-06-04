# Changelog — Puru T23

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
