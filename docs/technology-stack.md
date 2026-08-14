# Technology stack decision

Status: accepted for primary client development.

Decision date: 2026-08-13 (America/Los_Angeles).

## Decision

Build the primary mobile game as a top-down 2D title in Godot
`4.7.1-stable`. Treat the DApp as optional account, ownership, and settlement
integration outside the live match.

| Area | Selected direction | Status |
|---|---|---|
| Mobile game client | Godot `4.7.1-stable`, GDScript, 2D renderer, Compatibility backend | Active |
| Movement and collision | `CharacterBody2D` with authored `StaticBody2D` obstacles | Active |
| Tap navigation | `NavigationRegion2D`, `NavigationPolygon`, `NavigationAgent2D` | Active |
| Touch movement | Pinned MIT Virtual Joystick Godot runtime subset | Active |
| Camera | `Camera2D`, LOCK follow with forward framing, FREE drag pan | Active |
| Character presentation | `AnimatedSprite2D`, project-owned 176 x 216 semi-realistic business frames | Active |
| Match authority | Future Godot headless server sharing 2D map and rule data | Next networking slice |
| Existing TypeScript server | Room/rule/protocol reference; unchanged in this migration | Retain |
| Account and control API | Node.js 22, TypeScript, Fastify, Zod, guest-first sessions | Keep |
| Web DApp | React, TypeScript, Vite, viem | Optional |
| Contracts | Solidity, Foundry, OpenZeppelin, ERC-1155 skeleton | Optional |

The Unity `6000.3.7f1` client and its NGO Dedicated Server spike are retained
under `apps/unity-client/` with their build evidence. They are no longer the
primary client or networking direction. Do not implement new spatial gameplay
in both engines.

## Why Godot

Godot is the selected mature open-source 2D framework for this product because
it provides the complete interaction foundation in one MIT-licensed engine:

- a dedicated 2D renderer and 2D physics server instead of a 3D engine merely
  viewed from above;
- collision-aware character movement through `CharacterBody2D`;
- polygon baking, path queries, and path following through the built-in 2D
  navigation server;
- `Camera2D`, `AnimatedSprite2D`, multitouch events, responsive Control nodes,
  safe-area APIs, headless execution, and mobile exporters;
- scene/resource workflows that let maps and characters be replaced without
  replacing the movement engine;
- source availability and an MIT license, with no per-seat or per-install
  runtime fee.

The goal is not to write a general-purpose movement engine. Product code owns
the boardroom layout, interaction definitions, presentation, and game rules;
Godot owns the generic physics, pathfinding, rendering, input dispatch, camera,
animation, and UI mechanics.

## Alternatives considered

| Framework | Strength | Reason not selected |
|---|---|---|
| Defold | Small runtime, Lua, mature mobile export | Less complete editor-side 2D navigation and scene tooling for this map-heavy workflow |
| Cocos Creator | Mature 2D/mobile ecosystem and TypeScript | Larger framework surface and a less direct fit than Godot's built-in physics/navigation nodes |
| Phaser | Excellent browser-first 2D framework | Native Android/iOS packaging and device lifecycle are not its primary runtime model |
| LÖVE | Stable, small, permissive Lua framework | Requires assembling more editor, navigation, UI, and asset workflow infrastructure |
| GDevelop | Accessible open-source no-code workflow | Current repository needs code-owned authority, protocol integration, and deterministic automated tests |
| Unity 2D | Existing verified toolchain and broad ecosystem | Proprietary engine, larger mobile/runtime surface, and the current project was already accumulating custom glue around a 3D-first prototype |

This is a product-fit decision, not a claim that one engine is universally best.

## Interaction architecture

```text
touch joystick / desktop input -----+
                                     +--> PlayerController
tap target -> NavigationAgent2D -----+      |
                                            v
                                   CharacterBody2D.move_and_slide
                                            |
                          shared StaticBody2D room obstacles

Camera2D: LOCK -> follow player + forward framing
          FREE -> independent bounded touch drag
```

Rules enforced by the implementation:

1. Joystick and desktop debug input enter one normalized manual vector.
2. Any non-zero manual vector cancels active tap navigation immediately.
3. `NavigationAgent2D` chooses path points; only `CharacterBody2D` performs
   physical movement.
4. Boardroom table definitions generate physical colliders and navigation
   obstructions from one layout source calibrated to replaceable map art.
5. Touch controls are anchored independently from the world camera.
6. A visual interaction candidate is only presentation. A future multiplayer
   server must validate and accept the actual action.

## Multiplayer boundary

The committed Godot boardroom currently proves local movement and interaction.
It does not prove network authority. The next networking spike must use a
headless Godot process so the same 2D collision and map data can own movement,
then connect at least four independent clients.

The existing `services/game-server` remains unchanged as a tested reference for
rooms, private roles, ordered input, and snapshots. It must not become a second
production spatial authority. Account, allocation, moderation, inventory, and
post-match workflows may remain in Node/Fastify.

Before the multiplayer direction is accepted, one server and four clients must
demonstrate:

- server-owned spawn, movement, collision, and one context action;
- ordered input and rejection of speed, teleport, range, and cooldown cheats;
- 150 ms round-trip latency, 20 ms jitter, and 2 percent packet-loss testing;
- reconnect or clean rejection without duplicate players;
- owner-only private role/test state;
- CPU, memory, bandwidth, correction, latency, and disconnect measurements.

Do not add a wallet SDK, RPC request, or contract confirmation to this path.

## DApp boundary

Dependency direction remains one way:

```text
live match -> validated result/outbox -> account and entitlement service
                                              |
                                              v
                                    optional chain settlement

chain indexer -> ownership projection -> account/inventory UI outside a match
```

The chain may record ownership or infrequent settlement. It cannot own
movement, collision, matchmaking, voice, roles, cooldowns, meetings, votes, or
match results. Guest play remains the default.

## Pinned community dependency

The virtual joystick runtime subset comes from
`MarcoFazioRandom/Virtual-Joystick-Godot` commit
`b90891ee990881757105883677adbb619f4c319c` under MIT. Godot 4.7 introduced a
native class with the same name, so the imported global class is namespaced as
`FeatherfallVirtualJoystick`; its touch and output algorithm is unchanged.

The project setting `emulate_touch_from_mouse=true` exists only for desktop
development. `emulate_mouse_from_touch=false` prevents duplicate mobile input.

## Sources

- [Godot license](https://godotengine.org/license/)
- [Godot 2D introduction](https://docs.godotengine.org/en/4.7/getting_started/first_2d_game/index.html)
- [CharacterBody2D](https://docs.godotengine.org/en/4.7/classes/class_characterbody2d.html)
- [NavigationAgent2D](https://docs.godotengine.org/en/4.7/classes/class_navigationagent2d.html)
- [Using navigation agents](https://docs.godotengine.org/en/4.7/tutorials/navigation/navigation_using_navigationagents.html)
- [Multiple resolutions](https://docs.godotengine.org/en/4.7/tutorials/rendering/multiple_resolutions.html)
- [Virtual Joystick Godot](https://github.com/MarcoFazioRandom/Virtual-Joystick-Godot)

The Godot `4.7.1-stable` Linux binary used for initial validation was checked
against the release's official `SHA512-SUMS.txt` before use.
