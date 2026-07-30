# Metaverse DApp Product Repository

This product framework repository is coordinated by an external operations
workspace.

## Product boundaries

- Keep movement, collision, roles, voice, matchmaking, anti-cheat, and match
  results authoritative and off-chain.
- Guest play must remain functional without a wallet.
- Do not store wallet private keys or seed phrases in the Unity client.
- Do not invent chain IDs, RPC URLs, package identifiers, signing identities,
  deployment destinations, or store credentials.
- Mobile store compatibility takes precedence over token-gating, P2E,
  staking, externally purchased feature unlocks, or paid NFT loot boxes.

## Build discipline

- Unity is pinned by `ProjectSettings/ProjectVersion.txt`; package versions and
  the lock file must change together.
- The first successful editor resolution must generate
  `Packages/packages-lock.json`; commit it before the first shared build.
- Use the build methods in
  `apps/unity-client/Assets/MetaverseGame/Scripts/Editor/Build.cs`; do not guess
  an `-executeMethod`.
- Build distributable artifacts only from a clean, identified Git commit.
- Never commit Unity `Library/`, `Temp/`, `Logs/`, `obj/`, build artifacts,
  `.env` values, signing material, wallet secrets, or RPC credentials.
- Run `pnpm check` and the relevant Unity tests after changes.

`CLAUDE.md` must remain a symbolic link to this file.
