# Character Assets

## Godot 2D primary character

The primary Godot client uses twelve selected 16 x 16 four-direction walking
frames from Kenney's official RPG Urban Pack 1.0. The package's bundled notice
marks it Creative Commons Zero (CC0 1.0).

- Source: <https://kenney.nl/assets/rpg-urban-pack>
- Archive SHA-256:
  `4541d89d639fc7d1e905dd925e55b1c4977a41d983516228db1d57173bb9afaf`
- Runtime directory:
  `apps/godot-client/assets/characters/kenney-rpg-urban/`
- License copy: `LICENSE.txt` in the same directory
- Imported subset: three walking frames each for left, right, up, and down

The selected dark-haired office character is scaled by Godot's
`AnimatedSprite2D`. `PlayerController` selects direction from movement velocity
and holds the middle frame while idle. Character art is presentation only;
the `CharacterBody2D` collision shape and movement controller remain stable
when the art is replaced.

This is an explicit first-slice placeholder. Production character work should
retain the four-direction animation contract or update it deliberately with
tests, and must include redistribution rights suitable for the repository and
mobile binaries.

## Retained Unity 3D character

The Unity prototype uses `character-q.fbx` from Kenney's official Blocky
Characters 2.0 package under CC0 1.0. It is a formal black-suit, white-shirt,
red-tie variant with imported `idle` and `walk` clips.

| Asset | SHA-256 | Runtime role |
|---|---|---|
| `character-q.fbx` | `d6417d8ee0fe8d02386e2c0a144642cd3d1abeb1b9832f4c5d804f3b31faff0d` | Historical Unity player model |
| `Textures/texture-q.png` | `bf220bbc945072a4fce31118b6585207445ddac1c2836b350d10fe0d49fed5ca` | Historical Unity character atlas |

These 3D files remain for Unity build reproducibility. They are not the source
for the Godot 2D sprite and should not be expanded into a parallel primary
character pipeline.
