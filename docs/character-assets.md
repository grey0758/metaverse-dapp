# Character Assets

## Runtime Character

The default player visual is `character-j.fbx` from Kenney's official
[Blocky Characters](https://www.kenney.nl/assets/blocky-characters) package.
The package is marked CC0 1.0 in the included `Kenney-Blocky-Characters-
License.txt`; the source archive also documents 27 animations per character,
including `idle` and `walk`. The model is loaded at runtime below
`NetworkPlayer` so network ownership, transform replication, and the
`CharacterController` remain on the root object.

Kenney was retained after checking current community asset options because it
combines an explicit CC0 license, low-poly mobile geometry, one compact texture
atlas, business-attire variants, and locomotion clips in the same official
download. KayKit's CC0 animated packs remain a useful alternative, but their
fantasy character direction does not fit this boardroom as closely. Mixamo is
useful for animation retargeting, but does not provide the same simple CC0
redistribution contract for a bundled default character.

| Asset | Source | SHA-256 | Runtime role |
|---|---|---|---|
| `character-j.fbx` | Kenney Blocky Characters 2.0, official download | `05f192a180a8988043c04d5567d3249ea50b5bbce53cf913c6b16692d3beabea` | Business-attire low-poly player |
| `Textures/texture-j.png` | Same package | `11b6625d2d3119c9340fe93095374c45687be00fc7d1256c1f8db7d78cd7b531` | Imported character atlas |

The visual component plays the imported legacy `idle` and `walk` clips based
on replicated movement, normalizes the model to a two-meter gameplay height,
and keeps a procedural fallback for partial checkouts. EditMode coverage loads
the Resource, verifies both clips, and instantiates the runtime visual contract.
