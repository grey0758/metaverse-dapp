# Boardroom Assets

## Reference Room

The reference photographs are kept outside the product repository under the
ignored operations workspace path `tmp/柏拉图/`. They describe a long, bright
conference room with three rows of light honey-oak tables, black mesh chairs,
a cool gray carpet, left-side window bays, a warm vertical-slat wall, a cobalt
acoustic presentation wall, and a white modular ceiling.

The photographs are reference material only. Company logos, flags, names,
readable signage, and other identifying marks are not shipped in the game.

## Runtime Set

The Bootstrap scene loads these stable filenames from
`Assets/MetaverseGame/Resources/Boardroom/`. The first two filenames are kept
for compatibility with the earlier boardroom slice; their visual roles now
match the Plato room:

| Asset | Runtime size | Visual role | Android/iOS limit |
|---|---:|---|---:|
| `GraphiteStone.png` | 1024 x 1024 | pale cool-gray carpet | 1024 |
| `SmokedWalnut.png` | 1024 x 1024 | light honey-oak tables and panels | 1024 |
| `StrategyDisplay.png` | 2048 x 1152 | unbranded emissive display, 16:9 | 2048 |

The textures are RGB PNGs with mipmaps and are marked non-readable after
import. Standalone uses BC7; Android and iOS use ASTC 6x6. The editor
postprocessor in `Scripts/Editor/BoardroomAssetPipeline.cs` reapplies these
settings on every clean import. `BoardroomEnvironment` also has matching
procedural fallbacks for machines where a Resources import is unavailable.

Current runtime SHA-256 values:

| Asset | SHA-256 |
|---|---|
| `GraphiteStone.png` | `44d22f6b887fe76966238bef06c8a0d676a29214be70bfab08b4d4078daf6ce4` |
| `SmokedWalnut.png` | `c49f9c2d74473fdf04e7c8f559f6223a3002ebe570aaafbd20a81fab35e33046` |
| `StrategyDisplay.png` | `c18ffd20b6c90e7a3afc0a24258264c0c48bdee90667e6b6cd760c930cdf7ea8` |

## Generation Record

- Provider endpoint authorized for the attempt: `https://video.opencodex.uk/v1`.
- Requested models: `gpt-image-2` for 1K work and `gpt-image-2-4k` for large
  sources.
- Attempt date: 2026-08-12.
- Result: the provider was under excessive load; the 1K smoke request failed,
  the 4K request returned a bad-request response, and an edit request timed
  out. No provider source is claimed as a successful input for this revision.
- Selected runtime set: locally derived, reviewed safe fallback carpet and
  oak material maps, plus the previously reviewed unbranded abstract display
  bitmap. The ignored source photographs and generation scratch files are not
  product assets.

The material prompt intent was to preserve the room's pale gray woven carpet,
light honey-oak grain, and restrained corporate display composition while
excluding logos, flags, readable text, people, watermarks, room perspective,
and device frames. No provider credential, response URL, or source scratch
file is stored in Git or in this document.
