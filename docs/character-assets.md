# Character Assets

## Runtime Character

The default player visual is `character-q.fbx` from Kenney's official
[Blocky Characters](https://www.kenney.nl/assets/blocky-characters) package.
The package is marked CC0 1.0 in the included `Kenney-Blocky-Characters-
License.txt`; the source archive also documents 27 animations per character,
including `idle` and `walk`. The model is loaded at runtime below
`NetworkPlayer` so network ownership, transform replication, and the
`CharacterController` remain on the root object.

`character-q` was selected from the package's official per-character preview:
it is the formal black-suit, white-shirt, red-tie variant. The earlier
`character-j` candidate is a uniformed police character and is no longer used
as the boardroom default.

Kenney was retained after checking current community asset options because it
combines an explicit CC0 license, low-poly mobile geometry, one compact texture
atlas, business-attire variants, and locomotion clips in the same official
download. KayKit's CC0 animated packs remain a useful alternative, but their
fantasy character direction does not fit this boardroom as closely. Mixamo is
useful for animation retargeting, but does not provide the same simple CC0
redistribution contract for a bundled default character.

| Asset | Source | SHA-256 | Runtime role |
|---|---|---|---|
| `character-q.fbx` | Kenney Blocky Characters 2.0, official download | `d6417d8ee0fe8d02386e2c0a144642cd3d1abeb1b9832f4c5d804f3b31faff0d` | Formal-suit low-poly player |
| `Textures/texture-q.png` | Same package | `bf220bbc945072a4fce31118b6585207445ddac1c2836b350d10fe0d49fed5ca` | Imported character atlas |

The visual component plays the imported legacy `idle` and `walk` clips based
on replicated movement, normalizes the model to a two-meter gameplay height,
and keeps a procedural fallback for partial checkouts. EditMode coverage loads
the Resource, verifies both clips, and instantiates the runtime visual contract.
