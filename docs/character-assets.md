# Character Assets

## Godot 2D primary character

The primary Godot client uses sixteen project-authorized 176 x 216 RGBA frames
derived from user-supplied portraits of Dr. Chen Weilun. They preserve his
recognizable glasses, hairstyle, and formal navy business attire, with three
walking phases and one seated pose for down, left, up, and right.

- Runtime directory:
  `apps/godot-client/assets/characters/chen-weilun/`
- Source model: `gpt-image-2`
- Walking source sheet SHA-256:
  `d4174c9d086b3769ce841bf0dc4d74de88becb61d24e32f092a185366a5976fd`
- Seated source sheet SHA-256:
  `1e841d1c10ae9dcafcd715414d9d2cb9e6d76c878e54bdebe750c2a55edc3618`
- Runtime hashes and transformations: `PROVENANCE.md` in the same directory

`PlayerController` selects direction from movement velocity, holds the middle
walking frame while idle, and selects the chair's matching seated pose during
SIT. Character art is presentation only; the
`CharacterBody2D` collision shape and movement controller remain stable when
the art is replaced. Linear filtering and per-frame mipmaps preserve smooth
edges at the locked camera's fractional zoom.

The previous generic Featherfall business character remains in
`assets/characters/featherfall-business/` as the matching high-resolution
fallback without a real-person likeness. The Kenney RPG Urban Pack subset
remains in
`assets/characters/kenney-rpg-urban/` as an unreferenced low-resolution
fallback. Its archive SHA-256 is
`4541d89d639fc7d1e905dd925e55b1c4977a41d983516228db1d57173bb9afaf`,
and its bundled notice marks it Creative Commons Zero (CC0 1.0).

Future character work should retain the four-direction walking and seated
animation contract or update it deliberately with tests. Raw identity photos
must remain outside Git, and likeness-based derivatives must retain an explicit
authorization and redistribution boundary suitable for mobile binaries.

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
