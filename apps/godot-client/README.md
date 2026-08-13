# Featherfall Godot Client

This is the primary mobile game client. It is a landscape, top-down 2D slice
on Godot `4.7.1-stable`. The Unity client is retained as historical prototype
and build evidence under `../unity-client/`.

The framework owns physics movement, collision, navigation, camera, touch, and
UI. Product work should normally change boardroom art, collision outlines,
interaction zones, or character sprites without replacing these systems.

## Run

Open this directory with Godot `4.7.1-stable`, or run:

```bash
godot --path apps/godot-client
```

The checked-in test runner has no third-party test dependency:

```bash
godot --headless --path apps/godot-client --script res://tests/run_tests.gd
```

Desktop keys exist for development: WASD/arrows move, `E` interacts, and the
LOCK/FREE controls switch the camera. Mobile play uses the left virtual
joystick, tap-to-move on the room, right-side interaction button, and drag in
FREE mode.

No Android or iOS release export preset is committed until package identity,
signing references, and export templates are approved and pinned.
