# Architecture

The selected target stack and alternatives are recorded in
[technology-stack.md](technology-stack.md). The important product rule is that
real-time gameplay and optional blockchain ownership are separate systems.

## Runtime planes

```text
                          control and account plane
Unity mobile client ─────── HTTPS ───────> Fastify API
        │                                      │
        │ NGO / Unity Transport                ├── account, inventory,
        │                                      │   moderation, allocation
        v                                      │
Unity Dedicated Server ── post-match event ────┘
  movement, collision,
  roles, tasks, meetings

Web DApp ── HTTPS / EIP-1193 ──> API, indexer, settlement worker
                                           │
                                           v
                               optional EVM asset contracts
```

The Unity Dedicated Server is the target authority for one live session. The
client submits intent, never final state. The API controls identity and
long-lived product data but cannot decide a live movement or interaction.
Chain calls are outside the live-match process and are never required to play.

## Authority ownership

| State or action | Authority |
|---|---|
| Input sampling and local presentation | Owning Unity client |
| Movement, collision, spawn, doors, tasks, kills, reports | Unity Dedicated Server |
| Roles, cooldowns, meetings, votes, ejection, win condition | Unity Dedicated Server |
| Guest/platform account, bans, inventory read model, match history | Account API and durable database |
| Match allocation and reconnect lease | Account/control API plus session host |
| Voice room/team token | Backend; media remains with the selected voice provider |
| Wallet link and signature challenge | Account API |
| Asset ownership and approved settlement | EVM contract plus confirmation-aware indexer/worker |

Only the minimum result crosses from the match plane to the control plane. A
chain indexer may project ownership into account/inventory views, but that view
cannot change a match already in progress.

## Current scaffold versus target

The current TypeScript `services/game-server` remains a valid framework
prototype. It proves room state, private role delivery, ordered input, and a
fixed tick. It does not share Unity collision or map rules, so it must not grow
into a second production spatial authority.

The first NGO foundation now moves spawn, collision-integrated movement, and
one door interaction to a Unity Dedicated Server. It has a real Linux server
build and loopback startup proof, but has not passed the four-client impairment,
reconnect, private-state, or metrics gates. Keep the TypeScript tests as a
behavior reference until the C# rule and integration tests reach parity, then
retain Node only for control-plane responsibilities. Do not run both
implementations as authorities for one match.

## First vertical slice

1. Guest session creation through the API.
2. Direct connection to one local Unity Dedicated Server.
3. Server-owned room join, spawn, private role assignment, and match phase.
4. Sequence-ordered movement intent, server collision, and interpolated remote
   players.
5. One server-validated context action, then kill, report, meeting, vote, and
   win-condition rules.
6. Keyboard and mobile input driving the same gameplay commands.
7. Optional browser wallet challenge and EOA signature verification outside
   the match.
8. Owner-controlled ERC-1155 item minting skeleton with no gameplay powers.

The current framework has completed item 1, a local Dedicated Server form of
item 2, the spawn/movement foundation of item 4, and one door form of item 5.
Room/role authority is still only proven in the TypeScript reference, and the
Unity path has not passed the complete multiplayer acceptance gate. The
vertical slice does not claim production persistence, voice, matchmaking,
moderation storage, ERC-1271/6492 verification, store billing, chain
deployment, or signed mobile artifacts.

## Interaction contract

A client may highlight an interaction candidate locally, but it sends only an
attempt containing the target network ID, action, and input sequence. The
server checks:

- authenticated player and current connection ownership;
- match phase and alive/dead state;
- role and action permission;
- target existence and current interactable state;
- authoritative range and line of sight;
- cooldown, rate limit, and one-time-use/idempotency state.

Only the server changes the task, door, body, meeting, vote, or kill state.
Private role and task data use target-scoped messages rather than public state
hidden by client UI.

## Trust boundaries

- Unity, Web DApp, wallets, and all provider responses are untrusted inputs.
- Role assignment is sent only to the corresponding authenticated connection.
- Nonces are generated and consumed by the API; clients do not choose them.
- In-memory sessions are development-only and disappear on restart.
- Smart-contract and RPC calls are never submitted by the dedicated server
  during a live match.
- Asset metadata and 3D files remain off-chain; contracts store ownership and
  URI references only.

## Planned service extraction

The current in-memory services keep the first slice easy to run. Before public
multiplayer testing, add or extract:

- Unity Dedicated Server build and lifecycle endpoint;
- session allocation, connection tickets, and reconnect leases;
- account and inventory persistence;
- durable lobby and match-result records;
- moderation/audit events;
- result settlement jobs with idempotency keys;
- voice provider tokens issued by the backend;
- chain indexer and confirmation-aware settlement workers.
