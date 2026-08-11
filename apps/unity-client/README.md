# Unity Client

Pinned editor: `6000.3.7f1`.

Open this directory as the Unity project. The repository contains runtime
networking/gameplay code and an editor command that creates a low-poly
development scene without binary assets:

`Metaverse DApp > Create Development Scene`

The committed development scene starts a local NGO host by default. Its client
is landscape-mobile-first: Unity Input System feeds a safe-area-aware left
virtual joystick and right context-action button into the same authoritative
movement and door commands. Keyboard/gamepad bindings remain available only as
an editor or development-build fallback. Wallet SDK integration, production
art, voice, and account persistence remain later vertical slices.

Batch build entry points:

- `MetaverseGame.Editor.Build.PerformWindowsDevelopment`
- `MetaverseGame.Editor.Build.PerformAndroidDevelopment`

Pass `-buildOutput <absolute-path>`. A build also requires `BUILD_COMMIT` and
`BUILD_NUMBER` environment variables so an unidentifiable artifact cannot be
mistaken for a distributable build.
