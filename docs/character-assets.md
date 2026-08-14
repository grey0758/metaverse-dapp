# Character Assets

## Godot 2D primary character

The primary Godot client uses twelve project-owned 176 x 216 RGBA walking
frames generated for Featherfall. They depict one consistent adult business
professional in a charcoal suit, with three phases each for down, left, up,
and right.

- Runtime directory:
  `apps/godot-client/assets/characters/featherfall-business/`
- Source model: `gpt-image-2`
- Source sheet SHA-256:
  `2f96450d39ff528f5b2be27a8b8fc4501e63e2867bb22e3758c4632f1adbcc78`
- Runtime hashes and transformations: `PROVENANCE.md` in the same directory

`PlayerController` selects direction from movement velocity and holds the
middle frame while idle. Character art is presentation only; the
`CharacterBody2D` collision shape and movement controller remain stable when
the art is replaced. Linear filtering and per-frame mipmaps preserve smooth
edges at the locked camera's fractional zoom.

The previous Kenney RPG Urban Pack subset remains in
`assets/characters/kenney-rpg-urban/` as an unreferenced low-resolution
fallback. Its archive SHA-256 is
`4541d89d639fc7d1e905dd925e55b1c4977a41d983516228db1d57173bb9afaf`,
and its bundled notice marks it Creative Commons Zero (CC0 1.0).

Future character work should retain the four-direction, three-phase animation
contract or update it deliberately with tests, and must record source rights
suitable for the repository and mobile binaries.

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
