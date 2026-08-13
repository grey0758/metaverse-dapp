# Upstream

Runtime subset copied from
`MarcoFazioRandom/Virtual-Joystick-Godot` commit
`b90891ee990881757105883677adbb619f4c319c` (2025-01-19).

Godot 4.7 introduced a native class named `VirtualJoystick`. The imported
script's `class_name` is therefore changed to `FeatherfallVirtualJoystick`.
The scene's two texture UID hints are removed because those cache identifiers
are not portable outside the upstream project. No input, multitouch, dead-zone,
output logic, scene layout, texture content, or license text is changed.
Whitespace-only blank lines are normalized to satisfy repository checks.

Files included:

- `virtual_joystick.gd`
- `virtual_joystick_scene.tscn`
- `textures/joystick_base_outline.png`
- `textures/joystick_tip_arrows.png`

Do not update this dependency without pinning a new commit, reviewing its
touch behavior, reproducing its license, and rerunning the mobile input tests.
