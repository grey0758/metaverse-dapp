# Metaverse DApp Product Repository

This product repository is coordinated by the external operations workspace at
`/home/grey/work/ops-metaverse-dapp`.

## Product boundaries

- `apps/godot-client/` is the primary mobile game client. Build new maps,
  movement presentation, touch UX, and gameplay surfaces there.
- `apps/unity-client/` is retained historical evidence. Do not add new spatial
  gameplay to both engines or delete its verified build inputs without an
  explicit archival decision.
- Keep multiplayer movement, collision, roles, voice, matchmaking, anti-cheat,
  and match results authoritative and off-chain.
- Guest play must remain functional without a wallet.
- Never store a wallet private key, seed phrase, provider key, signing material,
  or RPC credential in any client or tracked file.
- Do not invent chain IDs, RPC URLs, package identifiers, signing identities,
  deployment destinations, or store credentials.
- Mobile store compatibility takes precedence over token-gating, P2E, staking,
  externally purchased feature unlocks, or paid NFT loot boxes.

## Godot discipline

- Godot is pinned by `apps/godot-client/.godot-version` to `4.7.1-stable`.
- Use Godot's `CharacterBody2D`, `NavigationAgent2D`/`NavigationRegion2D`,
  `Camera2D`, `AnimatedSprite2D`, and Control system. Do not replace mature
  engine physics, navigation, animation, or UI with custom equivalents.
- The MIT virtual joystick dependency is pinned and documented under
  `apps/godot-client/addons/virtual_joystick/`. Review and repin the upstream
  commit before changing it.
- Keep visual assets independent from collision, navigation, and interaction
  data so artists can replace maps and sprites without rewriting movement.
- Never commit `.godot/`, `builds/`, imported caches, export artifacts, signing
  material, or raw reference photographs.
- A mobile export is not validated until a real APK/AAB or iOS artifact exists
  and platform-specific checks pass. Desktop touch emulation is not an Android
  or iOS device test.

## Verification

- Build distributable artifacts only from a clean, identified Git commit.
- Run `GODOT_BIN=/path/to/godot pnpm godot:check` after Godot changes.
- Run `GODOT_BIN=/path/to/godot pnpm check` before a repository-wide commit.
- For visual changes, capture both 1280 x 720 and 960 x 540 landscape frames
  and inspect them for blank output, clipping, overlap, and missing assets.
- Preserve unrelated work and keep `CLAUDE.md` as a symbolic link to this file.
