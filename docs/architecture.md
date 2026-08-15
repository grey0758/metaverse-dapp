# Architecture

The selected client stack is recorded in
[technology-stack.md](technology-stack.md). Real-time gameplay and optional
blockchain ownership remain separate systems.

## Runtime planes

```text
                         control and account plane
Godot mobile client -------- HTTPS --------> Fastify API
        |                                        |
        | future authoritative game transport   +-- account, inventory,
        v                                        |   moderation, allocation
Godot headless match server -- result/outbox ----+
  movement, collision,
  roles, tasks, meetings

Web DApp -- HTTPS / EIP-1193 --> API, indexer, settlement worker
                                             |
                                             v
                                 optional EVM asset contracts
```

The Godot headless server is the target authority for a live session because it
can share the same 2D map, collision, and rule data as the client. The committed
client does not yet implement this network plane, so current local movement is
prototype behavior rather than a security boundary.

## Component ownership

| Component | Responsibility |
|---|---|
| `apps/godot-client` | Primary 2D mobile presentation, input, local prediction, maps, and client UX |
| Future Godot headless target | Authoritative spatial simulation and match rules |
| `services/game-server` | Retained TypeScript room/rule/protocol reference during migration |
| `services/api` | Guest sessions, optional wallet verification, account/control APIs |
| `packages/protocol` | Validated service wire schemas; adapt deliberately at the Godot boundary |
| `apps/web-dapp` | Optional browser wallet and account surface |
| `packages/contracts` | Optional ownership contracts, never live gameplay |
| `apps/unity-client` | Frozen 3D prototype and historical validation evidence |

## Client scene

The first Godot scene is deliberately layered:

```text
Boardroom
  BoardroomArt             replaceable 2D presentation
  NavigationRegion2D       engine-baked walkable polygons
  Obstacles                StaticBody2D collision generated from layout data
  MoveTargetMarker         client-only destination feedback
  Player                   CharacterBody2D
    NavigationAgent2D      engine path query and path progression
    AnimatedSprite2D       replaceable project-authorized character frames
  BoardroomCamera          LOCK/FREE Camera2D behavior
  GameHud                  safe-area touch and status controls
```

`BoardroomLayout` is the source for table rows, walkable bounds, collision
rectangles, navigation obstructions, spawn, and interaction points. Art code
consumes this data but does not decide where the player may walk.

## Authority ownership

| State or action | Current owner | Required multiplayer owner |
|---|---|---|
| Input sampling and local presentation | Owning Godot client | Owning Godot client |
| Camera and destination marker | Owning Godot client | Owning Godot client |
| Local boardroom movement/collision | Godot client prototype | Godot headless match server |
| Spawn, doors, tasks, kills, reports | Not implemented in Godot multiplayer | Godot headless match server |
| Roles, meetings, votes, win condition | TypeScript reference only | Godot headless match server |
| Account, bans, inventory, history | Account API and future database | Same |
| Wallet link and signature challenge | Account API | Same |
| Approved ownership/settlement | EVM contract plus indexer/worker | Same |

Only the minimum validated match result crosses from the match plane to the
control plane. Chain ownership cannot change a match already in progress.

## Interaction contract

A client may highlight a nearby object, but a multiplayer action contains only
intent and sequence. The server must validate:

- authenticated connection ownership;
- match phase and alive/dead state;
- role and action permission;
- target existence and authoritative state;
- authoritative distance and line of sight;
- cooldown, rate limit, and idempotency.

Only the server changes a task, door, body, meeting, vote, or kill. Private role
and task data must be target-scoped, not public state hidden by client UI.

## Migration rules

1. Keep the TypeScript server tests as a behavior reference.
2. Do not extend Unity and Godot spatial gameplay in parallel.
3. Build the first Godot headless authority with the same boardroom collision
   source as the client.
4. Prove four-client authority and impairment behavior before adding the full
   social-deduction loop.
5. Remove duplicate real-time authority only after parity tests pass; retain
   Node for control-plane responsibilities.

There must never be two production authorities for the same match state.
