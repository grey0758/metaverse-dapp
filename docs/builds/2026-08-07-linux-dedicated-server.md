# Linux Dedicated Server build record

This record captures the first runnable NGO Dedicated Server artifact. It is a
development spike, not a deployment or release approval.

## Source and toolchain

- source commit: `c0df31c92083fb337dfa5a1fb2ee82f94e7a0e54`;
- source branch at build time: `codex/slice-0.5-ngo`;
- worker: `greywin001`;
- Unity editor: `6000.3.7f1`;
- build target: `StandaloneLinux64`, subtarget `Server`;
- scripting backend: `Mono2x`;
- build number: `20260807.3`;
- build method: `MetaverseGame.Editor.Build.PerformLinuxServerDevelopment`;
- source checkout was clean before and after the successful build.

The pinned graph includes NGO `2.13.1`, Unity Transport `2.6.0`, Multiplayer
Tools `2.2.9`, Multiplayer PlayMode `2.0.2`, and the Unity Linux SDK/toolchain
packages at `1.1.0`.

## Artifact evidence

- player directory: 290 files, `175761963` bytes;
- launcher: `FeatherfallServer.x86_64`, `4808` bytes;
- launcher SHA-256:
  `c4361b92a96f764c885510343418f0a584dfd2592fb4f097ecb6e493d2419101`;
- Unity manifest: `558` bytes;
- Unity manifest SHA-256:
  `294642927b45e38365e968e3ed19098003c8d0e231b05a02e9d829aeaf02e326`;
- deployable archive:
  `FeatherfallServer-linux-x64-c0df31c.tar.gz`, `64009357` bytes;
- archive SHA-256:
  `e2c9fcc6e025dc8220234b73c03d3d7534a06eafc6ec130f8935df529e2194e4`.

The tar archive was produced from the complete Windows build directory with
normalized Linux permissions. The launcher is mode `0755`; directories are
`0755`; data files are `0644`.

## Verification

- Unity batch build exited `0` and logged `Build Finished, Result: Success`;
- final EditMode run: 5 total, 5 passed, 0 failed;
- test result size: `6270` bytes;
- test result SHA-256:
  `63085091772e3a2859ea43e5671e0e141d22586b7191e1951bd4fb8458d43957`;
- Linux workspace typecheck, protocol/API/game-server/contract tests, and all
  workspace builds passed;
- the final tar archive was extracted without permission repair;
- the extracted server started, logged `NGO Server started`, and listened on
  `127.0.0.1:17778/UDP` until the bounded smoke-test timeout stopped it.

No artifact was deployed to `sgp001`, no public listener was opened, and no
DNS, route, disaster-recovery, signing, wallet, or chain state changed.

## Remaining gate

This evidence proves buildability and single-process startup only. Slice 0.5
still requires four independent clients, the defined latency/jitter/loss
profile, correction and resource metrics, reconnect behavior, private-state
visibility, and C# room/role parity before the networking stack is accepted.
