# Boardroom Assets

## Reference Room

The reference photographs are kept outside the product repository under the
ignored operations workspace path `tmp/柏拉图/`. They describe a long, bright
conference room with four rows of light honey-oak tables, black mesh chairs,
a cool gray carpet, left-side window bays, a warm vertical-slat wall, a cobalt
acoustic presentation wall, and a white modular ceiling.

The source photographs are not shipped. The user explicitly authorized the
reference-room branding for this project, so the cropped display derivative
below retains the photographed company mark and names; the remaining raw
photos, people, and unrelated identifying details stay outside the product.

## Runtime Sets

### Godot 2D primary client

The primary client uses the following reviewed derivatives under
`apps/godot-client/assets/boardroom/`:

| Asset | Runtime size | Visual role | SHA-256 |
|---|---:|---|---|
| `plato-boardroom-hifi.png` | 2560 x 1440 | complete semi-realistic 2.5D room map | `6bc8f2396af7107452d4a6f8370bebdce2c807fb3563b7b13f5d62286f9f5830` |
| `plato-display.png` | 2048 x 1152 | authorized room display crop | `974395f57d0ca3c6eedb3b470c5c4def37b63e93ab7e74586460fabc918e7d09` |

`BoardroomArt` renders the complete map at one texture pixel per world unit and
perspective-maps the authorized display crop into the generated blank screen.
The display keeps its standby/active highlight interaction. The older
`carpet.png` and `oak.png` files remain as unreferenced fallback sources for
the retained procedural treatment; they are not layered over the high-fidelity
map.

The generated room art is still presentation-only. `BoardroomLayout` owns the
trapezoid walkable outline, four table obstacle rows, lectern, spawn, and
interaction points. `BoardroomNavigation` bakes from that data, while physics
uses the same obstacles plus a closed perimeter collider.

The active character set is under
`assets/characters/featherfall-business/`. It contains twelve 176 x 216 RGBA
frames: three walking phases for down, left, up, and right. Its source and
per-frame hashes are recorded in that directory's `PROVENANCE.md`. The former
CC0 Kenney frames remain available as an unreferenced low-resolution fallback.

Godot imports the high-resolution room, display, and business character with
mipmaps; the project canvas filter is linear with mipmaps. This removes the
nearest-neighbor pixel treatment used by the initial prototype.

### 2026-08-13 Godot generation

- Authorized endpoint: `https://video.opencodex.uk/v1`.
- Room source model: `gpt-image-2-4k`.
- Selected room source: 3840 x 2160 PNG, SHA-256
  `87718ec909f758011b1d2a6a7e6ed7a0fa3ab198a3705a805566d9a993006b77`.
- Room prompt intent: reproduce the supplied Plato room as a clean, empty,
  semi-realistic top-down 2.5D game map with four oak table rows, black mesh
  chairs, left windows, right oak slats, cobalt front wall, five approved
  ceremonial flags, and a blank presentation screen; exclude people, UI,
  watermarks, and generated text.
- Room transformation: reviewed 4K source downsampled with Lanczos filtering to
  2560 x 1440, metadata stripped, and stored as RGB PNG.
- Character source model: `gpt-image-2`.
- Character source: 1254 x 1254 PNG, SHA-256
  `2f96450d39ff528f5b2be27a8b8fc4501e63e2867bb22e3758c4632f1adbcc78`.
- Character prompt intent: one consistent adult East Asian business
  professional in a charcoal suit, arranged as a strict 3 x 4 sheet for three
  walk phases and four directions on a removable solid magenta background;
  exclude text, logos, props, extra figures, and pixel-art styling.
- Character transformation: soft chroma-key matte and despill, deterministic
  cell split, independent-shadow removal from the three back-facing frames,
  common 176 x 216 canvas, aligned foot baseline, and stripped metadata.

The user directed and authorized the generated room and character derivatives
for this project. Provider credentials, raw Plato photographs, generation
responses, rejected candidates, and source sheets remain outside Git in the
ignored operations scratch area.

The 2D client has been visually smoked at 1280 x 720 and 960 x 540 on Linux
X11 software rendering. That proves nonblank composition and responsive HUD
placement, not physical mobile-device performance.

### Retained Unity prototype

The Bootstrap scene loads these stable filenames from
`Assets/MetaverseGame/Resources/Boardroom/`. The first two filenames are kept
for compatibility with the earlier boardroom slice; their visual roles now
match the Plato room:

| Asset | Runtime size | Visual role | Android/iOS limit |
|---|---:|---|---:|
| `GraphiteStone.png` | 1024 x 1024 | pale cool-gray carpet | 1024 |
| `SmokedWalnut.png` | 1024 x 1024 | light honey-oak tables and panels | 1024 |
| `StrategyDisplay.png` | 2048 x 1152 | authorized reference-room display crop, 16:9 | 2048 |

The textures are RGB PNGs with mipmaps and are marked non-readable after
import. Standalone uses BC7; Android and iOS use ASTC 6x6. The editor
postprocessor in `Scripts/Editor/BoardroomAssetPipeline.cs` reapplies these
settings on every clean import. `BoardroomEnvironment` also has matching
procedural fallbacks for machines where a Resources import is unavailable.

Current runtime SHA-256 values:

| Asset | SHA-256 |
|---|---|
| `GraphiteStone.png` | `44d22f6b887fe76966238bef06c8a0d676a29214be70bfab08b4d4078daf6ce4` |
| `SmokedWalnut.png` | `a9e8cf255ee82fc0f0d173cffd504610c460a7be463e5df05fc97af27320ac87` |
| `StrategyDisplay.png` | `974395f57d0ca3c6eedb3b470c5c4def37b63e93ab7e74586460fabc918e7d09` |

## Unity visual calibration

The scene uses a lower shoulder-height third-person framing with a 64 degree
field of view and an 8 degree inward locked-camera offset. Its authored shell
is calibrated to a narrow 15.2 by 24 meter room instead of the earlier square
prototype. The room keeps the cobalt acoustic front wall, five left window
bays with roller blinds and a low city silhouette, right oak slat wall and door
opening, four forward-facing table rows with eight seats each, modular ceiling,
narrow recessed light bands, lectern, front speakers, and five ceremonial
banners as the primary landmarks.

`StrategyDisplay.png` is a deterministic crop of the user-authorized
`DSC01770.jpg` reference photograph. The 7008 x 4672 source remains outside the
product repository; only the stripped, palette-optimized 2048 x 1152 runtime
derivative is committed. Source SHA-256:
`4e5f2c4de0c2ea2cc89b1623e65e63ee8db7c5858348583d414db59589fbd6e7`.

- Provider endpoint authorized for the attempt: `https://video.opencodex.uk/v1`.
- Requested models: `gpt-image-2` for 1K work and `gpt-image-2-4k` for large
  sources.
- Attempt date: 2026-08-12.
- Result: the provider was under excessive load; the 1K smoke request failed,
  the 4K request returned a bad-request response, and an edit request timed
  out. No provider source is claimed as a successful input for this revision.
- Selected runtime set: the reviewed carpet and oak maps remain unchanged.
  The display uses the authorized reference crop described above. The 2 x 2
  oak tiling review showed no new edge seam. No generated provider output is
  used. The ignored source photographs and calibration scratch files are not
  product assets.

The earlier image-provider prompt intent was to preserve the room's pale gray
woven carpet and light honey-oak grain while excluding logos, readable text,
people, watermarks, and room perspective. The selected display is instead the
explicitly authorized deterministic photo crop described above. No provider
credential, response URL, or source scratch file is stored in Git or in this
document.
