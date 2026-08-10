# Vertical-slice roadmap

The project advances by playable, testable slices. Chain work never blocks the
guest game loop. The target networking choice and its measurable fallback gate
are in [technology-stack.md](technology-stack.md).

## Slice 0: framework

Current repository state:

- validated shared wire protocol;
- authoritative in-memory lobby, private roles, movement intents, and snapshots;
- guest session API and explicitly disabled-by-default wallet verification;
- Unity development-scene generator, socket reference client, committed NGO
  Bootstrap scene, EditMode tests, and Windows/Android/Linux Dedicated Server
  batch build methods;
- optional React DApp and undeployed ERC-1155 owner-mint skeleton.

Unity package resolution, C# compilation/tests, immutable commits, and the
canonical remote are complete. Application identities and signing references
remain player-release gates; a chain/domain remains optional and is needed
only if wallet authentication is enabled.

## Slice 0.5: authoritative network foundation

This is the next implementation slice:

- activate and open Unity `6000.3.7f1`, resolve packages, and commit
  `Packages/packages-lock.json`;
- pin NGO `2.13.1`, Multiplayer Tools, and Multiplayer Play Mode in one
  reviewed package change;
- produce a local Unity Dedicated Server target and direct-IP client
  connection;
- replace local transform authority with ordered input sent to the server;
- spawn at server-owned map points and collide against the same test-map
  geometry;
- replicate remote players with interpolation and correction metrics;
- add one server-validated door or task interaction;
- run four clients under the latency, jitter, and loss profile in the
  technology decision;
- keep the TypeScript server as a rule reference until C# parity tests pass,
  then remove its movement authority.

Current progress:

- NGO `2.13.1`, Multiplayer Tools `2.2.9`, Multiplayer PlayMode `2.0.2`,
  Unity Transport `2.6.0`, and the Windows-to-Linux toolchain are locked;
- the direct-IP bootstrap, server-owned spawn and movement, shared arena
  collision, remote transform replication, and one validated door interaction
  are implemented in the committed scene;
- session-ticket connection approval, reconnect-preserved role/spawn state,
  duplicate live-ticket rejection, and owner-only private role state are
  implemented pending the next Unity batch verification;
- five EditMode tests pass and a clean Linux Dedicated Server build starts and
  binds to loopback UDP on Linux;
- four-client impairment testing, correction metrics, restart/reconnect smoke,
  and C# room/role parity remain open.

Exit gate: the dedicated-server spike passes every acceptance check in
`technology-stack.md`. If it does not, benchmark Photon Fusion 2 with the same
scene and conditions before expanding the game loop.

## Slice 1: complete social-deduction loop

- lobby host controls and reconnect-safe player slots;
- deterministic map spawn points and task assignments;
- server-owned cooldowns, kill/report actions, body state, meetings, voting,
  ejection, and win conditions;
- private state sent only to the owning session;
- one low-poly test map with keyboard and mobile input;
- automated room-state transition and cheating-input tests.

Exit gate: at least four clients can complete one full match from lobby through
win condition without a wallet, with private state visible only to the owner
and every material action accepted or rejected by the server.

## Slice 2: multiplayer hardening

- replace development connection auth with API-issued, expiring, one-use
  session tickets;
- persistent account, moderation, and audit records;
- matchmaker and room leases with reconnect grace periods;
- message-rate limits, schema/version negotiation, idempotency, and abuse
  controls;
- provider-issued proximity/team voice tokens, never peer-generated authority;
- load, packet-loss, reconnect, and host-failure test matrix.

Do not adopt Kubernetes or Agones merely to complete this slice. First measure
one dedicated server's CPU, memory, bandwidth, startup time, and safe room
capacity, then select a hosting and orchestration model.

## Slice 3: mobile product

- touch movement, context actions, accessibility, haptics, and safe-area UI;
- mobile URP performance budgets, object pooling, addressable assets, and
  low-memory recovery;
- Android device build/test path on a registered Windows build worker;
- iOS Xcode archive/sign/device path on a registered macOS builder;
- account deletion, privacy, parental/age, crash, and analytics flows.

## Slice 4: optional ownership DApp

This slice must not delay Slices 0.5 through 3. Start it only after a chain, RPC
policy, public domain, custody model, and store-policy review are approved:

- CAIP-2/CAIP-10 account identity and EIP-1193 wallet provider boundary;
- SIWE-style sessions with EOA plus ERC-1271/ERC-6492 verification;
- ERC-1155/721 ownership and metadata indexing;
- confirmation-aware, idempotent reward/settlement jobs outside live matches;
- Web-only inventory/marketplace surfaces where mobile-store rules require it;
- no token balance, purchased NFT, or wallet connection required for play.

## Slice 5: production and release

- durable data stores, migrations, backups, observability, alerting, and
  incident runbooks;
- clean-checkout CI with pinned Unity/package locks and retained artifacts;
- contract audit, deployment approvals, multisig/role controls, and monitored
  indexing;
- real-device multiplayer, wallet deep-link, restore-session, upgrade, and
  store-review matrices;
- signed artifacts with commit, build number, editor/toolchain metadata, size,
  and SHA-256 retained.
