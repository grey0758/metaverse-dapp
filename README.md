# Metaverse DApp

Playable-framework repository for a mobile-first, top-down 2D social-deduction
game with an optional EVM asset layer.

The product is game-first:

- guest players can authenticate without a wallet;
- `apps/godot-client/` is the primary client, pinned to the MIT-licensed Godot
  `4.7.1-stable` engine;
- Godot supplies `CharacterBody2D` collision movement, `NavigationAgent2D`
  pathfinding, `Camera2D`, touch input, animation, and responsive UI;
- the first landscape slice contains the high-fidelity Plato boardroom, four
  obstacle-aware table rows, a semi-realistic four-direction business player,
  tap navigation, a multitouch virtual
  joystick, LOCK/FREE camera modes, and contextual interaction;
- the authoritative server must own multiplayer movement and match decisions;
  the current Godot boardroom is a local interaction slice, not proof of a
  production multiplayer server;
- the Unity 3D client remains in `apps/unity-client/` as a frozen historical
  prototype and verified build record, not the primary development target;
- the API, shared protocol, Web DApp, and undeployed ERC-1155 skeleton remain
  independent of the client engine.

No production chain, RPC URL, package identity, signing identity, or deployment
destination is guessed.

## Repository layout

```text
apps/
  godot-client/       Primary Godot 4.7.1 2D mobile client
  unity-client/       Retained Unity 3D prototype and historical build inputs
  web-dapp/           Optional wallet/account web surface
services/
  api/                Guest and wallet-auth HTTP API
  game-server/        TypeScript authority/protocol reference
packages/
  protocol/           Shared TypeScript wire schemas
  contracts/          Solidity ERC-1155 asset contract
infra/                Local container topology
scripts/              Verification helpers
docs/                 Architecture and development guidance
```

## First run

Requirements: Godot `4.7.1-stable`, Node.js 22, pnpm 10, and Git LFS. Docker is
needed only for the local service stack.

```bash
pnpm install --frozen-lockfile
GODOT_BIN=/path/to/Godot_v4.7.1-stable pnpm check
pnpm dev
```

Run the game from the repository root:

```bash
GODOT_BIN=/path/to/Godot_v4.7.1-stable
"$GODOT_BIN" --path apps/godot-client
```

Or import `apps/godot-client/project.godot` in the Godot project manager and
press Play. The default design size is 1280 x 720 landscape. Desktop keyboard
bindings are retained for development; mobile play uses touch.

See [development](docs/development.md),
[architecture](docs/architecture.md), and the
[technology decision](docs/technology-stack.md) before extending gameplay or
networking. The implementation order and release gates are in the
[vertical-slice roadmap](docs/roadmap.md).

The real Unity Windows/Android validation records remain available under
`docs/builds/` as historical evidence. They do not validate the Godot client.
