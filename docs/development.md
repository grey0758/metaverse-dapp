# Development

## Godot client

Use Godot `4.7.1-stable`. The exact project pin is in
`apps/godot-client/.godot-version`.

Open `apps/godot-client/project.godot` in the Godot project manager, or run
from the repository root:

```bash
GODOT_BIN=/path/to/Godot_v4.7.1-stable
"$GODOT_BIN" --path apps/godot-client
```

The main scene is `scenes/boardroom/boardroom.tscn`. It starts directly in the
playable landscape boardroom. Mobile controls are always visible in development
builds; desktop keyboard controls exist for debugging and automation.

| Input | Result |
|---|---|
| Left virtual joystick | Direct collision movement; cancels active path |
| Tap room floor | Engine navigation path to the closest walkable point |
| LOCK | Follow player with forward meeting-room framing |
| FREE | Unlock camera; drag the room to pan within bounds |
| SIT / STAND / USE | Run the currently available context action |
| WASD/arrows | Desktop-only movement debug input |
| `E`/Space | Desktop-only interaction debug input |
| `L` / `F` | Desktop-only LOCK / FREE debug input |

### Client checks

Set `GODOT_BIN` when the pinned editor is not on `PATH`:

```bash
GODOT_BIN=/path/to/Godot_v4.7.1-stable pnpm godot:check
```

This command verifies the editor version, imports resources in headless editor
mode, and runs the repository-owned test runner. The current suite covers 108
settings, layout, seat identity/occupancy, input locking, animation, navigation,
collision, camera, scene, foreground, HUD, and multitouch checks.

Run visual smoke under a real display or Xvfb after presentation changes:

```bash
xvfb-run -a -s '-screen 0 1440x900x24' \
  "$GODOT_BIN" --path apps/godot-client \
  --display-driver x11 --rendering-method gl_compatibility \
  --resolution 1280x720 \
  --script res://tests/capture_smoke.gd -- \
  --output=/tmp/featherfall-godot-1280x720.png
```

Repeat at 960 x 540 and inspect both images. Headless tests and desktop mouse
touch emulation do not validate Android/iOS safe areas, multitouch hardware,
rotation, lifecycle, performance, thermal load, or battery use.

Pass `--seat=seat_t01_north_06` after the script separator to exercise the
actual SIT flow before capture.

### Seating interaction

The chair workflow follows the established Godot community pattern: proximity
selects a chair, a per-chair anchor fixes the final pose, and a Tween aligns the
character before the seated animation is held. The layout stores anchor vectors
instead of adding 80 `Marker2D` scene nodes, but the ownership boundary is the
same. Relevant community discussions are:

- [How can I make my character sit on a chair?](https://forum.godotengine.org/t/how-can-i-make-my-character-sit-on-a-chair/39651)
- [Jittering CharacterBody3D when setting global position](https://forum.godotengine.org/t/jittering-characterbody3d-when-setting-global-position/137892)

SIT cancels navigation, zeroes velocity, rejects manual/tap movement, disables
the player collision shape, reserves the seat, and Tweens to its exact anchor.
STAND returns to the walkable approach point before restoring collision and
movement. The explicit local state is deliberately smaller than a generic FSM
plugin and leaves seat occupancy available to future server authority.

### Extending maps and assets

- Edit `BoardroomLayout` for walkable bounds, obstacles, spawn, and interaction
  points.
- Keep `BoardroomArt` presentation-only. Replacing a texture must not silently
  change collision or navigation.
- Let the navigation region bake from layout obstacle outlines. Do not maintain
  an unrelated hand-authored route graph.
- Replace `AnimatedSprite2D` frames without changing `PlayerController` input or
  movement behavior.
- Keep raw Plato photographs outside the product repository. Only reviewed
  derivatives with recorded provenance may ship.
- Pin and document every community dependency and license.

Do not add production Android or iOS export presets until application identity,
signing references, and export template versions are approved.

## Local services

```bash
pnpm install --frozen-lockfile
pnpm dev
```

Default development endpoints:

- game WebSocket reference: `ws://127.0.0.1:8787`
- account API: `http://127.0.0.1:8788`
- Web DApp: `http://127.0.0.1:5173`

The Godot client is not yet connected to the multiplayer service. The current
TypeScript `services/game-server` proves room, private-role, ordered-input, and
snapshot behavior, but it does not share the Godot 2D map physics. Keep it as a
reference until the Godot headless authority has parity; do not expand spatial
gameplay in both implementations.

Environment configuration remains uncommitted. No production chain is selected
in the repository.

## Repository checks

```bash
GODOT_BIN=/path/to/Godot_v4.7.1-stable pnpm check
pnpm contracts:test
```

`pnpm check` includes Node typechecks/tests/builds and the Godot import/test
suite. Contract tests remain independent.

## Multiplayer authority

The local player scene is a client interaction prototype. In multiplayer, a
client may predict presentation but only a server may:

- set authoritative position or spawn;
- decide traversal and collision;
- accept a task, door, kill, report, meeting, or vote;
- assign a private role or task;
- start or finish a match.

The next implementation target is a Godot headless match process sharing the
same boardroom collision and game-rule data. Do not claim authority until the
four-client impairment and reconnect gates in `technology-stack.md` pass.

## Retained Unity prototype

`apps/unity-client/` remains pinned to Unity `6000.3.7f1` with its historical
Windows, Android, and Linux Dedicated Server entry points. It is retained so
prior evidence remains reproducible; it is not the primary client.

Do not delete it, republish its artifacts as Godot results, or continue adding
new spatial gameplay there. Historical commands and evidence are preserved in
`apps/unity-client/README.md` and `docs/builds/`.

## DApp and contracts

Guest play remains the default. Contract deployment scripts stay absent until
a target chain, deployment owner, RPC secret reference, and confirmation policy
are approved. A game client or match server must not store a private key or
submit frame-by-frame chain calls.
