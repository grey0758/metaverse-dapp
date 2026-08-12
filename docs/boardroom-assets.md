# Boardroom Assets

## Runtime Set

The Bootstrap scene loads these authored textures from
`Assets/MetaverseGame/Resources/Boardroom/`:

| Asset | Runtime size | Usage | Android/iOS limit |
|---|---:|---|---:|
| `GraphiteStone.png` | 1024 x 1024 | floor and stone accents | 1024 |
| `SmokedWalnut.png` | 1024 x 1024 | table and walnut panels | 1024 |
| `StrategyDisplay.png` | 2048 x 1152 | emissive strategy display | 2048 |

The textures are RGB PNGs with mipmaps and are marked non-readable after
import. Standalone uses BC7; Android and iOS use ASTC 6x6. The editor
postprocessor in `Scripts/Editor/BoardroomAssetPipeline.cs` reapplies these
settings on every clean import. `BoardroomEnvironment` keeps the procedural
textures as a fallback when an authored resource is unavailable.

The runtime set is about 5.4 MiB as source PNGs before Unity platform
compression. It is deliberately derived from generated 4K sources rather than
shipping the 4K files in the player.

## Generation Record

- Provider endpoint: `https://video.opencodex.uk/v1`
- Generation mode: OpenAI-compatible Images Generations API with URL response
  download using the same scoped bearer authorization; no response URLs are
  stored in the repository.
- Model: `gpt-image-2-4k`
- Quality: `high`
- Generated: 2026-08-12
- Prompt constraints: no logos, watermark, readable text, letters, numbers,
  people, or device frames; the display is abstract and contains no readable
  labels.

### Graphite stone

Prompt summary: premium graphite stone surface texture for a high-end
executive conference room floor; flat orthographic material scan; square
edge-to-edge surface; even diffuse illumination; graphite and charcoal palette;
honed stone with subtle variation and fine pores; no grout, borders, text,
symbols, logos, watermark, rooms, furniture, perspective, dramatic cracks, or
glossy reflections.

Source: `graphite-stone-source.png`, 3840 x 2160, SHA-256
`749c6a7b568afff89ee504ff0edef00e022471ea0b3286182594aa95f7430a9a`.

Runtime derivative: `GraphiteStone.png`, SHA-256
`868b1f0be9fd0a244eac259685701f5d6a95a2d8e7a004a5b0b8a03979381aff`.

### Smoked walnut

Prompt summary: premium smoked walnut veneer for an executive conference table
and architectural wall panels; flat orthographic scan; square edge-to-edge
surface; vertical grain; even diffuse illumination; deep walnut and umber
palette; bookmatched low-gloss veneer; no seams, borders, text, symbols, logos,
watermark, rooms, furniture, perspective, oversized knots, or glare.

Source: `smoked-walnut-source.png`, 3840 x 2160, SHA-256
`8ecdc5bd62846e74c1867f8017a9a64a2d00bb1daa44cfa3c91cf4a723efd5df`.

Runtime derivative: `SmokedWalnut.png`, SHA-256
`a4bf4886894f684e0c8c3b558137bc41d1b5b768a773feccc27d06b11e5d6abf`.

### Strategy display

Prompt summary: elegant abstract strategy visualization for a large executive
boardroom display; dark digital command display; abstract world network and
balanced data modules; wide 16:9 layout; central map and restrained side
charts; self-illuminated near-black graphics in cyan, teal, amber, and coral;
no readable text, letters, numbers, flags, logos, watermark, room, people,
bezel, white background, or clutter.

Source: `strategy-display-source.png`, 3840 x 2160, SHA-256
`90b7defecf7a80e66da9a140f517ddf34873217ba36ec0617419a79ee0f2418b`.

Runtime derivative: `StrategyDisplay.png`, SHA-256
`c18ffd20b6c90e7a3afc0a24258264c0c48bdee90667e6b6cd760c930cdf7ea8`.

The ignored generation scratch files and API credential are not part of the
repository. Rotate the supplied provider credential after the generation
session.
