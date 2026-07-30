# Security and chain boundary

- Guest login remains the default mobile entry path.
- Wallet connection is optional and belongs in lobby, account, inventory, or
  settlement screens.
- The Unity client never stores a seed phrase or wallet private key.
- WalletConnect pairing URIs, signatures, bearer tokens, and RPC credentials
  must not enter logs or analytics.
- The API validates the configured domain, URI, chain identifier, nonce, and
  expiry before accepting a wallet signature.
- EOA verification in the development API is not sufficient for production.
  Add chain-aware ERC-1271 and ERC-6492 verification before smart-account
  release.
- Contract calls require explicit nonce/deadline/domain separation and a
  confirmation-aware backend job.
- Do not grant unlimited ERC-20 allowances by default.
- Do not use NFTs or wallet balances to unlock mobile gameplay, maps, powers,
  or matchmaking.
- Do not add P2E marketing, paid NFT random rewards, token prize pools, or
  staking without a new policy, legal, and store review.

Project-level RPC credentials and signing material belong in the approved
secret manager and are injected only at runtime. This repository stores names
and placeholders, never values.
