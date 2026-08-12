# Development

## Local services

```bash
pnpm install --frozen-lockfile
pnpm dev
```

Default development endpoints:

- game WebSocket: `ws://127.0.0.1:8787`
- Unity NGO direct connection: `127.0.0.1:7777/UDP`
- account API: `http://127.0.0.1:8788`
- Web DApp: `http://127.0.0.1:5173`

All can be overridden by a non-committed environment file. No production chain
is selected in the repository.

`services/game-server` is currently the runnable WebSocket authority prototype.
It proves the framework protocol but does not share Unity collision or map
physics. Read [technology-stack.md](technology-stack.md) before adding gameplay
actions to it.

## Unity

Use Unity `6000.3.7f1`. Open `apps/unity-client`, load
`Assets/MetaverseGame/Scenes/Bootstrap.unity`, and enter Play mode. The
committed scene starts a local host by default and contains:

- NGO `2.13.1` over Unity Transport;
- a 30 Hz `NetworkManager` and direct-IP bootstrap;
- connection approval with persistent session tickets and owner-only private
  role state;
- an immediate live-status HUD with a private role badge, local-player marker,
  and a compact session status;
- a mobile-friendly executive boardroom with procedural stone, walnut, glass,
  leather, metal, and display materials, collision-aware furniture, and warm
  architectural lighting;
- authored graphite stone, smoked walnut, and abstract strategy-display
  textures under `Assets/MetaverseGame/Resources/Boardroom/`, with procedural
  fallbacks and mobile platform import limits;
- a landscape-only, safe-area-aware mobile control layer with a left virtual
  floating joystick capture zone and right context-action button;
- a server-spawned network player prefab;
- ordered, server-integrated movement with `CharacterController` collision;
- server-authoritative `NetworkTransform` replication;
- one range, line-of-sight, sequence, and cooldown-validated door action.

`Metaverse DApp > Create Development Scene` regenerates tracked assets. Review
and commit its output before building; batch builds deliberately fail if the
scene is missing instead of mutating a clean checkout.

The TypeScript WebSocket client and prototype remain in the repository as a
rule reference, but the committed Bootstrap scene does not start them.

Repository-owned batch methods:

- `MetaverseGame.Editor.Build.PerformWindowsDevelopment`
- `MetaverseGame.Editor.Build.PerformAndroidDevelopment`
- `MetaverseGame.Editor.Build.PerformLinuxServerDevelopment`

Example after a clean Git commit exists:

```bash
UNITY_EDITOR -runTests -batchmode -nographics \
  -projectPath apps/unity-client \
  -testPlatform editmode \
  -testResults artifacts/unity-editmode.xml \
  -logFile artifacts/unity-editmode.log
```

Do not replace `UNITY_EDITOR` with a guessed path in shared automation. Machine
setup scripts resolve it from the pinned version. On Windows, use the
`Unity.com` console entry point so the caller receives the real process exit
code. Do not add `-quit` to a `-runTests` invocation; the Unity Test Framework
exits after writing the result file.

The Linux Dedicated Server build requires an identified clean revision:

```bash
BUILD_COMMIT="$(git rev-parse HEAD)" \
BUILD_NUMBER="local.1" \
"$UNITY_EDITOR" -batchmode -nographics -quit \
  -projectPath apps/unity-client \
  -executeMethod MetaverseGame.Editor.Build.PerformLinuxServerDevelopment \
  -buildOutput artifacts/linux-server/FeatherfallServer.x86_64 \
  -logFile artifacts/linux-server-build.log
```

The output is a directory, not only the small launcher executable. Preserve
`FeatherfallServer_Data/`, `MonoBleedingEdge/`, `UnityPlayer.so`, the launcher,
and its adjacent manifest together. A Linux deployment archive must preserve
directory traversal and executable permissions.

Dedicated Server builds force server mode. Runtime options are:

```text
-ip <server-address>        client destination, default 127.0.0.1
-listen-ip <bind-address>   server bind address, default 127.0.0.1
-port <udp-port>            default 7777
-mode host|client|server|manual
```

`UNITY_SERVER` overrides `-mode` to `server`. Keep the default loopback bind
for local work; selecting a private or public listener is an operations and
security decision, not a source-code default.

### Next Unity milestone

Completed foundation:

- package and Linux cross-toolchain locks are committed;
- direct local connection, server spawn, movement, shared collision, transform
  replication, and one validated door path are implemented;
- pure authority rules have EditMode coverage;
- a Linux Dedicated Server has been built from a clean commit and started on
  Linux with a loopback UDP listener.

The retained evidence for that build is in
[the first Linux Dedicated Server build record](builds/2026-08-07-linux-dedicated-server.md).

Remaining acceptance work:

1. Test one server plus at least four independent clients with Multiplayer
   Play Mode or separate player processes.
2. Add correction metrics and evaluate movement under 150 ms RTT, 20 ms
   jitter, and 2 percent packet loss.
3. Add reconnect or explicit clean rejection without duplicate players.
4. Prove owner-scoped private test state.
5. Record CPU, memory, bandwidth, corrections, latency, packet loss, and
   disconnect behavior before accepting NGO for the full game loop.

Do not delete the current TypeScript prototype until this path passes and its
room/role behavior has equivalent C# tests. Do not continue adding spatial
gameplay to both implementations.

### Input and interaction

Unity Input System is the active input backend. The mobile client writes a
normalized left-stick value and context-action presses into one input router;
that router feeds the existing NGO movement RPC and server-validated door RPC.
Keyboard and gamepad bindings are retained only in the editor and development
builds for automation and desktop debugging. They do not fork gameplay rules.

Android and iOS are configured for auto-rotation between the two landscape
orientations only. Touch controls are anchored through `Screen.safeArea` so
notches, rounded corners, and display cutouts do not cover the joystick or
action button.

Clients may predict presentation, but only the server may:

- set an authoritative position or spawn;
- decide collision and traversal;
- accept a task, door, kill, report, meeting, or vote;
- assign a private role or task;
- start or finish a match.

## Contracts

The contract package uses Foundry and the pinned OpenZeppelin npm dependency:

```bash
pnpm contracts:test
```

Deployment scripts are intentionally absent until a target chain, deployment
owner, RPC secret reference, and confirmation policy are approved.

Contract and wallet development can proceed independently of NGO, but it stays
behind the guest game loop in release priority. The Unity Dedicated Server must
not import a wallet SDK or call an RPC endpoint.

## Remote build worker

Use a clean checkout on a separately managed Windows build worker. It must not
receive local Unity `Library`, `Temp`, `Logs`, or `obj` folders. A real build
requires a clean commit/tag, a Unity license, the pinned editor/modules, and an
artifact manifest with SHA-256.

The retained `greywin001` evidence for commit `87ef2b0` is in the
[2026-08-12 Windows and Android build record](builds/2026-08-12-windows-android-boardroom.md).
It covers 16 passing EditMode tests, texture import validation, a Windows
development player, an arm64 IL2CPP Android development APK, and a real player
screenshot. It does not replace Android physical-device touch, safe-area,
lifecycle, performance, thermal, or battery testing.
