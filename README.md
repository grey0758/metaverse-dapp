# Metaverse DApp

Playable-framework repository for a low-poly, mobile-first 3D social-deduction
game with an optional EVM asset layer.

The first vertical slice is deliberately game-first:

- guest players can authenticate without a wallet;
- the authoritative game server owns rooms, roles, movement, and match state;
- the Unity client contains a committed NGO development scene, an authored
  executive boardroom, landscape touch controls, direct-IP bootstrap,
  server-owned movement and door interaction, plus repository-owned player and
  Dedicated Server build entry points;
- the Web DApp exposes optional wallet connection and SIWE-style challenge
  plumbing without selecting a production chain;
- the contract package contains an owner-operated ERC-1155 collection skeleton;
- no chain ID, RPC URL, package name, signing identity, or deployment
  destination is invented.

## Repository layout

```text
apps/
  unity-client/       Unity 6000.3.7f1 client
  web-dapp/           Optional wallet/account web surface
services/
  api/                Guest and wallet-auth HTTP API
  game-server/        Authoritative WebSocket game loop
packages/
  protocol/           Shared TypeScript wire schemas
  contracts/          Solidity ERC-1155 asset contract
infra/                Local container topology
scripts/              Verification and build helpers
docs/                 Architecture and development guidance
```

## First run

Requirements: Node.js 22, pnpm 10, Git LFS, and Docker for the local service
stack. Unity work uses the editor pinned in
`apps/unity-client/ProjectSettings/ProjectVersion.txt`.

```bash
pnpm install --frozen-lockfile
pnpm check
pnpm dev
```

Then open `apps/unity-client` in Unity Hub and load the committed
`Assets/MetaverseGame/Scenes/Bootstrap.unity` scene. The editor defaults to a
local host on UDP `7777`; command-line clients can join it directly. Use
`Metaverse DApp > Create Development Scene` only when intentionally
regenerating the tracked spike scene and prefab.

Copy `.env.example` to a non-committed local environment only when a setting is
needed. Guest mode works without chain settings. Wallet verification remains
disabled until an explicit CAIP-2 chain, RPC URL, and public domain are
configured.

See [development](docs/development.md) and
[architecture](docs/architecture.md) before adding gameplay or chain features.
The implementation order and release gates are in the
[vertical-slice roadmap](docs/roadmap.md). The first runnable Dedicated Server
evidence is in the
[2026-08-07 build record](docs/builds/2026-08-07-linux-dedicated-server.md).
The paired Windows and Android boardroom validation is in the
[2026-08-12 build record](docs/builds/2026-08-12-windows-android-boardroom.md).
