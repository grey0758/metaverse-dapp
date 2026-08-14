# Vertical-slice roadmap

The project advances through playable, testable slices. Chain work never blocks
the guest game loop.

## Slice 0: Godot 2D client foundation

Completed:

- Godot `4.7.1-stable` project pinned under `apps/godot-client`;
- 1280 x 720 landscape configuration with responsive 960 x 540 layout;
- 2560 x 1440 semi-realistic Plato boardroom presentation with four table rows,
  chairs, meeting screen, lectern, windows, slat wall, and authorized branding;
- `CharacterBody2D` collision movement and generated `StaticBody2D` furniture;
- `NavigationRegion2D` baking plus `NavigationAgent2D` tap-to-move;
- pinned MIT multitouch joystick; manual input cancels automatic navigation;
- LOCK follow and bounded FREE drag `Camera2D` modes;
- semi-realistic four-direction business player with walking/seated poses and
  contextual SIT, STAND, and USE actions;
- 80 reservable chair anchors plus foreground ordering for flags, lectern,
  tables, and chairs;
- 107 automated checks covering project settings, layout, seat occupancy,
  movement locking, navigation, collision, camera, scene resources, HUD,
  foreground ordering, and injected multitouch;
- nonblank seated X11 visual smoke at 1280 x 720 and 960 x 540.

This slice is local. It does not claim Android/iOS device behavior or
multiplayer authority.

## Slice 0.5: Godot authoritative network foundation

- create a headless Godot match target sharing boardroom collision and rule data;
- connect at least four independent clients to one server;
- move spawn and physical movement authority off the client;
- preserve ordered input and add correction/interpolation metrics;
- implement one server-validated context interaction;
- add reconnect or clean duplicate-session rejection;
- prove owner-only private test state;
- test at 150 ms RTT, 20 ms jitter, and 2 percent packet loss;
- record CPU, memory, bandwidth, corrections, latency, and disconnect behavior;
- retain the TypeScript room/rule tests until Godot parity exists.

Exit gate: every network check in `docs/technology-stack.md` passes from one
identified commit. Do not expand the full game loop before this gate.

## Slice 1: complete social-deduction loop

- lobby host controls and reconnect-safe player slots;
- deterministic spawn and task assignment;
- server-owned cooldowns, kill/report, body state, meetings, voting, ejection,
  and win conditions;
- private state sent only to the owning session;
- automated state-transition and cheating-input tests;
- one complete match from lobby through win condition without a wallet.

## Slice 2: map and asset production

- extend the reviewed production background pipeline to additional rooms while
  preserving layout collision contracts;
- create additional rooms and transitions from authorized references;
- extend the coherent business character set with full locomotion and gameplay
  state animations;
- establish texture, atlas, audio, memory, and load-time budgets;
- add localization-ready UI and accessibility options.

## Slice 3: multiplayer hardening

- API-issued expiring one-use connection tickets;
- persistent account, moderation, audit, lobby, and match-result records;
- matchmaking, room leases, reconnect grace periods, and abuse controls;
- backend-issued proximity/team voice tokens;
- load, packet-loss, reconnect, lifecycle, and host-failure matrices;
- measured hosting choice after one server's room capacity is known.

## Slice 4: mobile product

- approved Android and iOS package identities and signing references;
- pinned Godot export templates and clean-checkout export scripts;
- physical-device touch, safe-area, rotation, lifecycle, reconnect, performance,
  thermal, battery, and low-memory tests;
- haptics, account deletion, privacy, age/parental, crash, and analytics flows;
- verified APK/AAB, Xcode archive, and store artifacts with hashes.

## Slice 5: optional ownership DApp

Start only after chain, RPC policy, domain, custody, and store-policy approval:

- CAIP-2/CAIP-10 identity and EIP-1193 provider boundaries;
- server-issued SIWE-style challenges with replay protection;
- ERC-1155/721 ownership indexing;
- idempotent, confirmation-aware post-match settlement;
- Web-only marketplace surfaces where store policy requires it;
- no wallet, token, or purchased NFT required for play.

## Slice 6: production release

- migrations, backups, observability, alerts, and incident runbooks;
- clean-checkout CI with pinned engine/dependencies and retained artifacts;
- real-device multiplayer and upgrade/restore matrices;
- contract audit and deployment approvals if chain features ship;
- signed artifacts tied to commit, build number, engine version, timestamp,
  size, and SHA-256.
