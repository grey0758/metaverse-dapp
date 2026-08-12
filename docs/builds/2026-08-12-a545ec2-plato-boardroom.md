# Windows and Android Plato boardroom evidence

This record captures the paired development players built from the Plato
boardroom revision. It is reproducibility and visual-smoke evidence, not a
release approval.

## Source and worker

- source commit: `a545ec2bad3765af0756daaef288cf00bdbe07dd`;
- source branch at build time: `main`;
- worker: `greywin001`, registered Tailscale host
  `grey-win-honor.tail239026.ts.net`;
- Unity editor: `6000.3.7f1`;
- build methods:
  `MetaverseGame.Editor.Build.PerformWindowsDevelopment` and
  `MetaverseGame.Editor.Build.PerformAndroidDevelopment`;
- the worker checkout was verified at the source commit before artifact
  inspection and remained the identified product checkout throughout the
  smoke run.

Retained worker evidence is below
`D:\work\metaverse-dapp-ops\artifacts\unity\a545ec2`. Unity `Library/`,
`Temp/`, `Logs/`, and `obj/` were not transported from the Linux workspace.

## Pre-build verification

- workspace `pnpm check`: passed;
- boardroom texture validation: 3 of 3 textures passed;
- EditMode result: 17 total, 17 passed, 0 failed, 0 skipped;
- EditMode result SHA-256:
  `9af1f27448da7a0c1d676fe3521985742ac4706395cdf846e7b3fc3c75b2d7b6`;
- Unity import log contains `Validated 3 boardroom textures.`.

Runtime texture hashes at this revision:

| Asset | SHA-256 |
|---|---|
| `GraphiteStone.png` | `44d22f6b887fe76966238bef06c8a0d676a29214be70bfab08b4d4078daf6ce4` |
| `SmokedWalnut.png` | `c49f9c2d74473fdf04e7c8f559f6223a3002ebe570aaafbd20a81fab35e33046` |
| `StrategyDisplay.png` | `c18ffd20b6c90e7a3afc0a24258264c0c48bdee90667e6b6cd760c930cdf7ea8` |

## Windows development player

- target: `StandaloneWindows64`, subtarget `Player`;
- scripting backend: `Mono2x`;
- build number: `a545ec2-windows-1`;
- build manifest timestamp: `2026-08-12T19:45:21.9508670Z`;
- launcher: `Featherfall.exe`, `667648` bytes;
- launcher SHA-256:
  `87f775393ac5ef273e9e6d20c8b768f5d4c41b2fbf10d536b32dbfe961cac490`;
- launcher manifest SHA-256:
  `5cda84bafabd2ec2286b9dfcf006fdd9a51e4b3ff138084a54c98bc80ddfa206`;
- build manifest reports `184284610` total bytes for the player output.

The player was started in the active physical-console Session 1 with
`-screen-fullscreen 0 -screen-width 1280 -screen-height 720 -force-d3d11`.
The same Session 1 task observed a visible `UnityWndClass` window with an
active HWND before capture. The full desktop capture is retained at
`D:\work\metaverse-dapp-ops\artifacts\unity\a545ec2\window\boardroom-session1-live.png`:

- capture dimensions: `1920 x 1080`;
- capture SHA-256:
  `2dc35881e3ef17694a4fcaab65550d08e702ceb82a8aa9d7cd77f6287ce28fac`;
- derived game-window review crop: `1264 x 680`;
- derived crop SHA-256:
  `705c9a67fc6fc69b203d888846841713945721ad338d927e46af36663d21fef9`.

The capture shows the textured boardroom, three honey-oak table rows, chairs,
window bays, local avatar marker, `YOU / DUCK` label, private-role HUD,
floating joystick, and `USE` action control. The controls remain inside the
landscape safe area and do not overlap the session HUD. This proves Windows
rendering and layout only; it is not physical Android touch evidence.

## Android development APK

- target: `Android`, subtarget `Player`;
- scripting backend: `IL2CPP`;
- build number: `a545ec2-android-1`;
- build manifest timestamp: `2026-08-12T20:06:16.6023530Z`;
- APK: `Featherfall.apk`, `51947814` bytes;
- APK SHA-256:
  `d8339cfff184f23b5c2646298c14bac8d47af8a4b22f87e5d0be39bb71319491`;
- APK manifest SHA-256:
  `69faad2ced26fdfa8ef62728fc98bf314c2735324e1804add5053d475a03e1a3`;
- ABI: `arm64-v8a`;
- minimum SDK: 25;
- target SDK: 36;
- package: `com.DefaultCompany.Featherfall`;
- launch activity: `com.unity3d.player.UnityPlayerGameActivity`;
- `aapt dump xmltree` reports `android:screenOrientation` value `0xb`,
  equivalent to `userLandscape`;
- Unity's bundled JDK plus `apksigner` verified APK Signature Scheme v2 with
  one debug signer and exited `0`.

The package identifier and debug signer are development defaults. This APK is
not a release candidate and no release keystore or store identity was used.

## Asset provenance and limits

The supplied room photographs remain reference material outside the product
repository. The shipped scene excludes logos, flags, readable signs, and
other identifying marks. The image-provider attempt on 2026-08-12 did not
produce a successful source image, so this revision does not claim provider-
generated source art; it uses the reviewed local runtime derivatives recorded
in [boardroom assets](../boardroom-assets.md). No provider credential, URL
response, or scratch source was stored in Git.

No Android physical device was connected. Touch ownership, safe-area behavior
on cutouts, rotation, lifecycle, reconnect behavior, frame time, memory,
thermal load, and battery use remain pending. Approved application IDs,
release signing, store configuration, and the broader multiplayer acceptance
matrix also remain release gates.

No DNS, route, public listener, power policy, signing store, wallet, chain,
Curator disaster-recovery, or `sgp001` workload state changed during this
validation. The temporary Windows capture tasks were removed after the
visible-player capture; the development player may remain open for inspection.
