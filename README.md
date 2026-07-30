# Metaverse DApp

Playable-framework repository for a low-poly, mobile-first 3D social-deduction
game with an optional EVM asset layer.

The first vertical slice is deliberately game-first:

- guest players can authenticate without a wallet;
- the authoritative game server owns rooms, roles, movement, and match state;
- the Unity client contains a generated development scene, socket client,
  movement prototype, and repository-owned batch build entry points;
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

Then open `apps/unity-client` in Unity Hub. Run
`Metaverse DApp > Create Development Scene` once if the generated bootstrap
scene is not present.

Copy `.env.example` to a non-committed local environment only when a setting is
needed. Guest mode works without chain settings. Wallet verification remains
disabled until an explicit CAIP-2 chain, RPC URL, and public domain are
configured.

See [development](docs/development.md) and
[architecture](docs/architecture.md) before adding gameplay or chain features.
The implementation order and release gates are in the
[vertical-slice roadmap](docs/roadmap.md).
