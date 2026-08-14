# Featherfall Business Character Provenance

## Walking Source

- Generated: 2026-08-13
- Provider endpoint: `https://video.opencodex.uk/v1`
- Model: `gpt-image-2`
- Source canvas: 1254 x 1254 RGB PNG
- Source SHA-256:
  `2f96450d39ff528f5b2be27a8b8fc4501e63e2867bb22e3758c4632f1adbcc78`
- Authorization: the user directed and authorized generation and project use.

The prompt requested one consistent adult East Asian business professional in
a charcoal suit, rendered as a non-pixel-art, semi-realistic 3 x 4 walking
sheet. Rows are down, left, up, and right; columns are left-foot-forward,
neutral, and right-foot-forward. A solid magenta key background was requested
with no text, logo, watermark, props, extra figures, floor, or cast shadow.

## Seated Source

- Generated: 2026-08-14
- Provider endpoint: `https://video.opencodex.uk/v1`
- Model: `gpt-image-2`
- Source canvas: 1254 x 1254 RGB PNG
- Source SHA-256:
  `6aca2129599bd8333b2051559abcb45a07b43ce9c3f6522ae834a8a27e57f1d6`
- Transparent working derivative SHA-256:
  `8b42fea3c40206be77e587b12fc1d133b88b41787893abfaf9623d57c2803589`
- Authorization: the user directed and authorized generation and project use.

The seated prompt retained the same adult East Asian business professional,
charcoal suit, teal tie, hairstyle, proportions, and semi-realistic rendering.
It requested one stable seated pose facing down, left, up, and right on a solid
`#f205f3` key background, without chairs, text, logos, props, extra people,
floor, watermark, or cast shadow.

## Transformation

The ignored source sheets were processed with the project image pipeline's
soft chroma-key matte and despill. Walking art was split on the generated grid,
uniformly scaled, centered on 176 x 216 transparent canvases, and aligned at a
common foot baseline. Three disconnected back-view shadow components were
removed. The four seated figures were split, centered, and manually aligned to
the established character root so changing animation does not move the player.
PNG metadata was stripped. Godot imports every runtime frame with mipmaps.

## Runtime Hashes

| Frame | SHA-256 |
|---|---|
| `walk-down-0.png` | `8b59077bc2b58880775d2bcffbf649788c5b26e2c390461428b7e4852ef632fc` |
| `walk-down-1.png` | `bf91c1d022767c3eb0d93fcf8332a0a91a8304d3d6e0f647821348ba5a26aa78` |
| `walk-down-2.png` | `dcf07d0762136406d15e2cdb9808df6ba4cff044d59bc3e38feaa7c17b7a698f` |
| `walk-left-0.png` | `7017a9d56329da375318e31ddb7c871a2bbc423d69c66dbfe4c29cc7b167cb39` |
| `walk-left-1.png` | `a8e53d25879b4f24e260e1074590ef0c542f1388959b2fe2fb9a6202ec5a7bb7` |
| `walk-left-2.png` | `4e13d7a7a833265b78756d5dc3dcf7bb985aa11f8b6bf135e208616dc25af91d` |
| `walk-right-0.png` | `a0dd84efbb20add60ace708b083c9c0951cf0ce3ada9756abeabbbdcb22b8301` |
| `walk-right-1.png` | `197c8e4b2d12cc8f0d76f4b7b974d90cf1668f9b0e3a6fd02c5ea92c57952e72` |
| `walk-right-2.png` | `85f12150b65706743243c53b6bc08ab11580080f20b6352ea198e3af3157c17b` |
| `walk-up-0.png` | `d68b876edfd931b5ad7f2d0aed7284f0a9aee78a59ccc6ae5af46add0671417a` |
| `walk-up-1.png` | `1d21630cf6e90ce6dadf1e8a695764726ec886072e96cb8efdebeb9e73189513` |
| `walk-up-2.png` | `b9a96dcb07073a944aa6f23992ede5a1005d074665dc62a09d00ee02c764bfd5` |
| `sit-down.png` | `334551b04d54a51ab505974a97c9934dff02180908c9b5bc478218fb7ad31e83` |
| `sit-left.png` | `932e6be8a6238f20fd90993311b1be587b05889bae46b09eb582ff90184de1fd` |
| `sit-right.png` | `c66c74aa7617614cb837e657caa47c135e3e2f72162e42ffe93db47cb2fa74ed` |
| `sit-up.png` | `e77710295fd3e7779e915a47e0eafd39830be58bf68919bc993cdba48e040497` |

Provider credentials, response JSON, source sheets, and rejected candidates are
not product assets and must not be committed or copied to build workers.
