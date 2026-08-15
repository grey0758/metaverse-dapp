# Chen Weilun Protagonist Provenance

## Rights And Identity References

- Primary reference: user-supplied high-resolution portrait of Dr. Chen
  Weilun dated 2025-06-09, SHA-256
  `8a8d4cee93a920a2b8fcfb376e91b0b24bedc433e4dd517bc89f91fca4967160`.
- Supporting reference: user-supplied orange-background business portrait of
  the same person, SHA-256
  `9dfe5fd2ec230a55d93ee80a24d7acaf9d17180efb6080e4dd17696061c02d2c`.
- Authorization: the user supplied the photographs and explicitly authorized
  using them to create the current game protagonist.

Only the accepted game-scale derivatives are committed. The raw portraits are
not repository assets and must not be copied to build workers or releases.
This authorization does not make the person's likeness a reusable stock asset
or grant standalone redistribution outside this project.

## Walking Source

- Generated: 2026-08-14
- Provider endpoint: `https://video.opencodex.uk/v1`
- Model: `gpt-image-2`
- Source canvas: 1024 x 1536 RGB PNG
- Source SHA-256:
  `d4174c9d086b3769ce841bf0dc4d74de88becb61d24e32f092a185366a5976fd`
- Transparent working derivative SHA-256:
  `5508b1a675c0275b95c888843b9a2bd72aba284dc9496410293dd299bfb8619a`

The walking prompt used the two authorized portraits as identity and wardrobe
references and the previous Featherfall business sheet only as a pose, scale,
and rendering-style reference. It requested a strict 3 x 4 sheet: down, left,
up, and right rows with left-foot-forward, neutral, and right-foot-forward
columns. It preserved the side-parted black hair, thin metal-frame glasses,
dark navy-charcoal suit, white shirt, navy dotted tie, and restrained lapel
pin. A flat `#f205f3` key background was required with no text, watermark,
props, extra people, floor, or shadow.

## Seated Source

- Generated: 2026-08-14
- Provider endpoint: `https://video.opencodex.uk/v1`
- Model: `gpt-image-2`
- Source canvas: 1254 x 1254 RGB PNG
- Source SHA-256:
  `1e841d1c10ae9dcafcd715414d9d2cb9e6d76c878e54bdebe750c2a55edc3618`
- Transparent working derivative SHA-256:
  `451ed4ab0ce611388536f28e72f8da536b67f1901edc9c8594e701335372c39d`

The seated prompt retained the same identity and wardrobe in a strict 2 x 2
sheet: down, left, up, and right. Each pose sits upright without rendering a
chair, floor, prop, text, watermark, shadow, or additional person.

## Transformation

The ignored source sheets were processed with a soft chroma-key matte and
despill. Walking figures were split into twelve cells, normalized to a common
201-pixel figure height, centered on 176 x 216 transparent canvases, and
aligned to the established foot baseline. Seated figures were uniformly
scaled to 31 percent and placed at the established direction-specific seat
root offsets. PNG metadata was stripped. Collision, navigation, seat anchors,
and movement code are unchanged by the art replacement.

## Runtime Hashes

| Frame | SHA-256 |
|---|---|
| `walk-down-0.png` | `11821f81f2d185427cb6fa5c98f0417e8b12b509a2b57b8d0efc78ce5bbab9a0` |
| `walk-down-1.png` | `d413e213c17f840d4ccdea95bcb51923e3530bcd2ef02f575789af04ef238e1e` |
| `walk-down-2.png` | `b86936548263b2f3c3e980426e466f25e66ba136925f50d5a71f0d44f993fddf` |
| `walk-left-0.png` | `2489b83f0af53a4591425332385f0be456a7d7a469d1c4602c36ec58b9520599` |
| `walk-left-1.png` | `7772d00b76af491ec7a9cfaff9d1dda5aadd6cedfb134c721432188e2f5452ea` |
| `walk-left-2.png` | `c93d5a50c6eddbba651b468780aa8b7b16c999da33abb90c381fe0cf4fe94aab` |
| `walk-right-0.png` | `701a97c28e2e34a46d94b714c70e7870f322d19517dc5747a7073de5dedcf539` |
| `walk-right-1.png` | `61550f2257be727cced4e2a3893222ee21ed98a9787e8c75b0b20916ef806bac` |
| `walk-right-2.png` | `c12454f8f5c4bcc74471449fc7d6be6fe6b556acfca776a7702900d1e9520b26` |
| `walk-up-0.png` | `1f7c56dc546572da0775fdb70260ac5834a301d8dd928d6258f2d56fc5b0a0b5` |
| `walk-up-1.png` | `5db8782336cd1ce20f1e6247016bcb5c24198acb94c8524f5faa43b40ae363c7` |
| `walk-up-2.png` | `8784c794cee68efa926616910064ff4bfc37d8ce4bbc76f84f6990f46177ae0d` |
| `sit-down.png` | `3c12274517e5f52f66a2d461b756569b376f174b19301fac6f2dd91fe5b98f47` |
| `sit-left.png` | `21bfc0374336f7509325be13a5ded3614bc76e46a8382ac4014e1b0d5c5343e2` |
| `sit-right.png` | `668592bec1ff92cc8057f21e7ce1037e8fdd511425906d25a32d70c76991219f` |
| `sit-up.png` | `a4afed3aea50ba0a32b86ef0be93e96b2a0ff29d556c9cdb7f03d57aca174a0d` |

Provider credentials, response JSON, source sheets, prompts, raw photographs,
and rejected candidates are not product assets and must not be committed.
