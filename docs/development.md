# Development

## Local services

```bash
pnpm install --frozen-lockfile
pnpm dev
```

Default development endpoints:

- game WebSocket: `ws://127.0.0.1:8787`
- account API: `http://127.0.0.1:8788`
- Web DApp: `http://127.0.0.1:5173`

All can be overridden by a non-committed environment file. No production chain
is selected in the repository.

`services/game-server` is currently the runnable WebSocket authority prototype.
It proves the framework protocol but does not share Unity collision or map
physics. Read [technology-stack.md](technology-stack.md) before adding gameplay
actions to it.

## Unity

Use Unity `6000.3.7f1`. Open `apps/unity-client`, then run:

1. `Metaverse DApp > Create Development Scene`
2. open `Assets/MetaverseGame/Scenes/Bootstrap.unity`
3. enter Play mode while the game server is running

The generated scene currently exercises local movement and the prototype
socket. It is not evidence that NGO, a Dedicated Server build, remote transform
replication, or Unity-side collision authority works.

Repository-owned batch methods:

- `MetaverseGame.Editor.Build.PerformWindowsDevelopment`
- `MetaverseGame.Editor.Build.PerformAndroidDevelopment`

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

### Next Unity milestone

Do this only after Unity activation and the first successful package
resolution:

1. Commit the generated `Packages/packages-lock.json` with the existing
   manifest before changing the network dependency graph.
2. In one reviewable change, add the exact NGO and development-tool package
   pins recorded in `technology-stack.md` and resolve a new lock file.
3. Keep input, pure rules, network adaptation, and presentation in separate
   assemblies. A pure rule test must not require a socket or scene.
4. Add a dedicated-server bootstrap that accepts an API-issued development
   ticket or an explicitly local direct connection.
5. Test one server plus at least four independent clients with Multiplayer
   Play Mode or separate player processes.
6. Implement one complete path: move intent, server collision, replicated
   transform, context query, server-accepted door/task action.
7. Record CPU, memory, bandwidth, corrections, latency, packet loss, and
   disconnect behavior before accepting the stack.

Do not delete the current TypeScript prototype until this path passes and its
room/role behavior has equivalent C# tests. Do not continue adding spatial
gameplay to both implementations.

### Input and interaction

The installed Input System is the target abstraction for keyboard, gamepad, and
touch. The current `Input.GetAxisRaw` code is temporary. All devices should
produce the same move and context-action commands; mobile controls must not
fork the game rules.

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
