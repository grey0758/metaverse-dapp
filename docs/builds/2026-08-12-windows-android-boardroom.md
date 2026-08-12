# Windows and Android boardroom build record

This record captures the first paired Windows and Android development players
for the authored Featherfall boardroom and landscape mobile-control slice. It
is build and visual-smoke evidence, not a release approval.

## Source and worker

- source commit: `87ef2b05d9efc3e8bad1fb6c5dbd1d420f78a6b1`;
- source branch at build time: `main`;
- worker: `greywin001`, registered Tailscale host
  `grey-win-honor.tail239026.ts.net`;
- Unity editor: `6000.3.7f1`;
- source checkout was clean before and after validation and both builds;
- build methods:
  `MetaverseGame.Editor.Build.PerformWindowsDevelopment` and
  `MetaverseGame.Editor.Build.PerformAndroidDevelopment`.

The retained worker evidence is below
`D:\work\metaverse-dapp-ops\artifacts\unity\87ef2b0`. The Windows root keeps
its existing historical workspace name; no worker path or toolchain layout was
changed during this validation.

## Pre-build verification

- repository-wide `pnpm check` passed, including TypeScript checks, Foundry,
  protocol, API, game-server, and Web DApp builds and tests;
- Unity boardroom import validation exited successfully and logged
  `Validated 3 boardroom textures.`;
- EditMode result: 16 total, 16 passed, 0 failed, 0 skipped;
- EditMode result SHA-256:
  `9df3b5ff024d4c7c2e73271ba78956ed152a60cb2e2c00db5510f7de904f72df`.

The validated runtime textures were:

| Asset | SHA-256 |
|---|---|
| `GraphiteStone.png` | `868b1f0be9fd0a244eac259685701f5d6a95a2d8e7a004a5b0b8a03979381aff` |
| `SmokedWalnut.png` | `a4bf4886894f684e0c8c3b558137bc41d1b5b768a773feccc27d06b11e5d6abf` |
| `StrategyDisplay.png` | `c18ffd20b6c90e7a3afc0a24258264c0c48bdee90667e6b6cd760c930cdf7ea8` |

## Windows development player

- target: `StandaloneWindows64`, subtarget `Player`;
- scripting backend: `Mono2x`;
- build number: `87ef2b0-windows-1`;
- manifest timestamp: `2026-08-12T14:23:56.8723341Z`;
- complete player directory: 318 files, `184276539` bytes including the
  adjacent manifest;
- launcher: `Featherfall.exe`, `667648` bytes;
- launcher SHA-256:
  `87f775393ac5ef273e9e6d20c8b768f5d4c41b2fbf10d536b32dbfe961cac490`;
- manifest SHA-256:
  `b8b511a41d2f28e0c0633628c26e99fd99a130e96bc9b2449c10c6f0576493f4`.

The player launched in a bounded landscape window and remained responsive.
The retained `1034 x 614` game crop is
`artifacts/unity/87ef2b0/window/boardroom-game.png` in the upper operations
workspace, with SHA-256
`6bcf5fdf7c86e0381c1543d89ac11b36b3e52e14d935bba87369eac58fbcdd6b`.
It shows a nonblank textured boardroom, the local avatar and `YOU / DUCK`
marker, the compact session HUD, the private-role panel, the floating
joystick, and the `USE` button without the removed tutorial strip.

This screenshot proves rendering and layout only. A later keyboard movement
capture was obscured by another desktop application and is not retained as
movement evidence. Keyboard input remains a development fallback and is not
touch evidence.

## Android development APK

- target: `Android`, subtarget `Player`;
- scripting backend: `IL2CPP`;
- build number: `87ef2b0-android-1`;
- manifest timestamp: `2026-08-12T14:27:54.1938670Z`;
- APK: `Featherfall.apk`, `67689414` bytes;
- APK SHA-256:
  `05be8dddd2aadc6fa4eadbc64b5901b37bda6f67fd9e7e3de32679b55a54becb`;
- manifest SHA-256:
  `a2303f3ba935754d2fabb6de1c9db51ff041ade98ca9ae99c68c05a67799592c`;
- ABI: `arm64-v8a`;
- minimum SDK: 25;
- target SDK: 36;
- launch activity: `com.unity3d.player.UnityPlayerGameActivity`;
- activity orientation: Android value `11` (`userLandscape`);
- APK Signature Scheme v2 verification passed with one Android debug signer.

The package remains the unapproved Unity default
`com.DefaultCompany.Featherfall`. No release keystore, store signing identity,
or approved application identifier was used, so this APK is not a release
candidate.

## Remaining validation

No Android physical-device validation occurred. Touch ownership, safe-area
behavior on actual cutout displays, orientation changes, background and
foreground lifecycle, reconnect behavior, frame time, memory, thermal load,
and battery use remain pending. A release also still requires approved package
identifiers, signing references, store configuration, and the broader
multiplayer acceptance matrix.

No DNS, route, public listener, signing store, power policy, Curator disaster
recovery state, or `sgp001` workload changed during this build validation.
