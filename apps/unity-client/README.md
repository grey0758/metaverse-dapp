# Unity Client

Pinned editor: `6000.3.7f1`.

Open this directory as the Unity project. The repository contains runtime
networking/gameplay code and an editor command that creates a low-poly
development scene without binary assets:

`Metaverse DApp > Create Development Scene`

The generated scene connects to `ws://127.0.0.1:8787` by default and supports a
local keyboard movement prototype. Mobile controls, wallet SDK integration,
production art, voice, and account persistence remain later vertical slices.

Batch build entry points:

- `MetaverseGame.Editor.Build.PerformWindowsDevelopment`
- `MetaverseGame.Editor.Build.PerformAndroidDevelopment`

Pass `-buildOutput <absolute-path>`. A build also requires `BUILD_COMMIT` and
`BUILD_NUMBER` environment variables so an unidentifiable artifact cannot be
mistaken for a distributable build.
