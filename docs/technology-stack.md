# Technology stack decision

Status: accepted for the first multiplayer spike, subject to the measurable
exit gate below.

Research snapshot: 2026-07-29 (America/Los_Angeles).

## Decision

Build the game as a normal server-authoritative Unity multiplayer title. Treat
the DApp as an optional account, ownership, and settlement integration outside
the live match.

| Area | Selected direction | Timing |
|---|---|---|
| Mobile game client | Unity `6000.3.7f1`, GameObjects/MonoBehaviours, URP, Input System, `CharacterController` | Now |
| Real-time networking | Netcode for GameObjects (NGO) `2.13.1` over Unity Transport | Next spike |
| Match authority | Unity Dedicated Server build using the same spatial rules and game-rule assemblies as the client | Next spike |
| Account and control API | Node.js 22, TypeScript, Fastify, Zod, guest-first sessions | Keep |
| Durable service data | PostgreSQL when in-memory state is retired; Redis only when leases, queues, or multi-instance fan-out require it | Later |
| Voice | Managed provider with backend-issued room/team tokens; Vivox is the first integration candidate, not yet selected | After the game loop |
| Web DApp | React, TypeScript, Vite, viem; add Reown AppKit/wagmi only when the wallet UX needs them | Optional |
| Contracts | Solidity, Foundry, OpenZeppelin; ERC-1155 for cosmetic or collectible ownership | Optional |
| Game-server hosting | Local direct connection first; select managed hosting or Agones only after a measured dedicated-server build exists | Later |

The DApp does not determine the engine, transport, movement model, physics,
voice, or match architecture. Dependency direction must remain one way:

```text
live match -> validated match result/outbox -> account and entitlement service
                                                |
                                                v
                                      optional chain settlement

chain indexer -> ownership projection -> account/inventory UI outside a match
```

No wallet provider, RPC call, token balance, or contract confirmation is
allowed in the movement, interaction, meeting, voting, or win-condition path.

## Why the current WebSocket loop is not the target

`services/game-server` is useful as a protocol and authority proof. It already
tests room membership, private roles, ordered input, and a fixed server tick.
It is not yet a production 3D simulation:

- Unity moves a `CharacterController` through a physics scene, while the
  TypeScript server integrates unbounded `x/z` coordinates with no map
  collision.
- Doors, walls, ramps, line of sight, kill distance, body interaction, and
  navigation would have to be implemented twice in C# and TypeScript.
- JSON over WebSocket is suitable for the prototype but provides none of the
  object replication, unreliable delivery, interpolation, reconciliation, or
  network diagnostics expected from a real-time game stack.
- Maintaining two implementations of the spatial rules would create an
  exploitable disagreement between client and server.

The migration must be incremental:

1. Keep the TypeScript server and its tests as the known-good room/rule
   reference while the Unity networking spike is built.
2. Prove direct connection from two clients to one Unity Dedicated Server,
   server-owned spawning, movement, collision, and one context interaction.
3. Move role and match-phase rules into engine-independent C# assemblies that
   can run in EditMode tests and in the dedicated server.
4. After parity tests pass, remove real-time movement authority from the
   TypeScript service. Reuse Node/Fastify only for account, session allocation,
   moderation, inventory, and post-match workflows.

There must never be two production authorities for the same match state.

## Why NGO is the first choice

The first target is a small social-deduction room, currently capped at 12
players, not a high-player-count shooter:

- NGO `2.13.1` officially supports Unity 6.0 and later, Mono and IL2CPP, Linux,
  Windows, macOS, iOS, and Android.
- The current project already uses GameObjects, MonoBehaviours, and
  `CharacterController`; Netcode for Entities would add ECS conversion cost
  without solving a current scale problem.
- A Unity Dedicated Server can execute the same colliders, map data, and
  interaction queries as the client build.
- The official Boss Room and Bitesize samples provide maintained reference
  patterns for small-session co-op networking.
- NGO does not force a hosting or per-concurrent-user vendor decision before
  the core game is fun.

NGO is not assumed to be sufficient merely because it is official. Its
prediction and lag-compensation facilities are less complete than Photon
Fusion's. The spike must pass the network impairment gate before the project
commits more gameplay code to it.

Candidate package pins for that spike are:

| Package | Researched version | Rule |
|---|---:|---|
| `com.unity.netcode.gameobjects` | `2.13.1` | Pin directly |
| `com.unity.transport` | `2.6.0` through NGO | Do not override until Unity resolves the complete graph |
| `com.unity.multiplayer.tools` | `2.2.9` | Development diagnostics |
| `com.unity.multiplayer.playmode` | `2.0.2` | Multi-client editor testing |
| `com.unity.services.multiplayer` | `2.3.0` | Deferred; not needed for direct-IP gameplay |

These are research inputs, not an instruction to bypass Unity package
resolution. Add them only in the activated pinned editor, review the resolved
versions together, and commit the generated `Packages/packages-lock.json`.

## Authority and interaction model

The client sends intent. The dedicated server decides the result.

```text
local input
  -> input command with sequence/tick
  -> server simulation and collision
  -> authoritative state
  -> snapshots/events
  -> client interpolation and local presentation
```

For a context action such as task use, door use, kill, report, or meeting:

1. The client may highlight a nearby candidate for responsiveness.
2. It sends `TryInteract(targetNetworkId, action, sequence)`.
3. The server validates authentication, match phase, alive/dead state, role,
   target existence, distance, line of sight, cooldown, and one-time use.
4. The server changes state and emits the minimum public and private events.
5. The client only renders the accepted result.

Private roles, team-only state, task assignments, and hidden cooldowns use
owner- or target-scoped messages. They must not exist in a public replicated
object and then be hidden only by the UI.

Start with a 30 Hz authoritative simulation. Treat that as a hypothesis to
measure, not a permanent magic number. Input sampling, snapshot delivery, and
render interpolation may run at different rates.

## Spike acceptance gate

Before implementing the full social-deduction loop, the same small test map
must demonstrate:

- one desktop dedicated server and at least four independent clients;
- server-owned spawn positions and capsule collision against the same walls;
- smooth local and remote movement under 150 ms round-trip latency, 20 ms
  jitter, and 2 percent simulated packet loss;
- no client-authoritative teleport, speed, interaction-distance, or cooldown
  decision;
- one server-validated door/task interaction;
- reconnect or explicit clean rejection without duplicating a player;
- private test state visible only to its intended client;
- repeatable EditMode tests for pure game rules and PlayMode/integration tests
  for ownership and interaction validation;
- captured CPU, memory, bandwidth, correction count, and disconnect metrics.

If NGO cannot meet this gate without a disproportionate custom prediction
layer, implement the same scene and gate in Photon Fusion 2 before adding more
gameplay. FishNet is the second non-Unity candidate if its custom license is
accepted. Choose from measured behavior and total operating cost, not feature
lists.

## Alternatives considered

| Stack | Useful strengths | Main concern here | Disposition |
|---|---|---|---|
| Photon Fusion 2 | Prediction, compression, lag compensation, dedicated-server and host topologies | Proprietary service/licensing and greater vendor coupling | Measured fallback |
| FishNet 4 | Server-authoritative design, prediction, rollback, lag compensation, no CCU cap | Custom source-available license and smaller ecosystem | License-reviewed fallback |
| Mirror | Mature MIT project, large community, many transports | Less direct Unity 6/MPS alignment; current main docs still recommend older Unity LTS versions | Not selected |
| Nakama | Mature account/social backend and fixed-tick authoritative match runtime | Unity physics and spatial rules still need a separate server implementation | Future meta-service candidate |
| Colyseus | Productive TypeScript rooms, matchmaking, state synchronization, Unity SDK | Same duplicate-physics problem as the current server for 3D authority | Protocol alternative only |
| Netcode for Entities | High-scale ECS networking and official 128+ player sample | Unnecessary DOTS complexity for a 12-player GameObject game | Revisit only after profiling |
| Custom WebSocket | Full control and already running | Must build transport semantics, prediction, tooling, and spatial parity ourselves | Prototype only |

## Community and maintenance signal

This dated snapshot is a maintenance/adoption signal, not a quality ranking:

| Project | GitHub stars | Latest release seen | Recent activity seen |
|---|---:|---|---|
| NGO | 2,316 | `2.13.1`, 2026-07-24 | 2026-07-29 |
| Mirror | 6,269 | `v96.11.1`, 2026-07-26 | 2026-07-26 |
| FishNet | 1,989 | `4.7.2`, 2026-04-17 | 2026-04-17 |
| Nakama | 13,005 | `3.40.0`, 2026-07-13 | 2026-07-24 |
| Colyseus | 7,138 | `0.17`, 2026-02-06 | 2026-07-24 |

Photon Fusion is not meaningfully comparable by GitHub stars because its core
SDK is not developed as one of these public repositories.

## DApp boundary

The existing TypeScript/EVM choices are compatible with any Unity networking
choice:

- Keep guest or platform account login as the default.
- Link a wallet as an additional credential, never as the player identity
  required to enter a match.
- Verify wallet challenges in the API with server-issued nonces and explicit
  domain, URI, chain, expiry, and replay checks.
- Project indexed ownership into a normal database/read model. A mobile client
  reads that model outside the live match instead of querying an RPC endpoint.
- Submit any approved reward or ownership write through an idempotent outbox
  and confirmation-aware worker after the match.
- Use ERC-1155 for optional collectibles. Do not introduce an ERC-20 merely
  because the product is called a DApp.
- Keep marketplace and transfer flows in the Web DApp when mobile-store rules
  make native presentation risky.

The chain can record ownership or infrequent settlement. It cannot own
movement, collision, matchmaking, voice, roles, cooldowns, votes, or match
results.

## Sources

Primary product and platform sources:

- [NGO 2.13 documentation](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/index.html)
- [Unity Dedicated Server manual](https://docs.unity3d.com/6000.3/Documentation/Manual/dedicated-server.html)
- [Unity Multiplayer Services SDK](https://docs.unity.com/en-us/mps-sdk)
- [Unity Boss Room sample](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop)
- [Unity Bitesize samples](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.bitesize)
- [Photon Fusion introduction](https://doc.photonengine.com/fusion/current/fusion-intro)
- [FishNet documentation](https://fish-networking.gitbook.io/docs)
- [FishNet license](https://github.com/FirstGearGames/FishNet/blob/main/LICENSE.md)
- [Mirror documentation](https://mirror-networking.gitbook.io/docs)
- [Nakama authoritative multiplayer](https://heroiclabs.com/docs/nakama/concepts/multiplayer/authoritative/)
- [Colyseus documentation](https://docs.colyseus.io/)
- [Agones overview](https://agones.dev/site/docs/overview/)

GitHub repository and release metadata were read through the public GitHub API
on the research date.

Curator continuity sources:

- framework session `019faef5-4b94-72a0-b729-3edc475b58a8`;
- original mobile DApp standards session
  `019fad1c-4b33-7e40-9083-bb92dfcd9698`;
- this documentation action `fd319122-8363-461c-9744-8f9c5be5f5e2`.

Curator query keywords:

`metaverse-dapp Unity multiplayer networking interaction authoritative server
DApp boundary` and `Unity Goose Goose Duck social deduction multiplayer
blockchain iOS Android game architecture technology stack`.
