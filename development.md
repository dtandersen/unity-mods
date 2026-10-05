# Station Automation & Guidance Engine (SAGE) development

## Projects

```
StationeersAi.Common/ netstandard2.0   Shared bits used on both sides (trace log)
StationeersAi.Core/   netstandard2.0   Unity-free game data (models, tools, commands, chat, window)
Caev/               netstandard2.0   Generic agent engine (loop, clients, messages, tool ports)
StationeersAi.Test/   net8.0           TUnit + Shouldly + ArchUnitNET specs (game, chat, window)
Caev.Test/          net8.0           Engine specs: loop, activity, OpenAI wire (stub tools, fake server)
StationeersAi.Mod/    net48            BepInEx shell: plugin entry, patches, log (no game logic)
StationeersAi.Game/   net48            Unity/game adapters: SageMod lifecycle, Stationeers* readers, window, HTTP server
```

The split is deliberate: **the engine (`Caev`) sees game data
only as per-turn arguments and references `Common` alone**; the domain
(`Core`) may use the engine but never the game. None of the three touch Unity or Stationeers types, so `StationeersAi.Test` can specify and test
them without the game running. Game reads live in `StationeersAi.Game`:
`SageMod` owns load/unload per level, the `Stationeers*` readers pull
`Prefab.AllPrefabs`, the `ElectronicReader` recipe tables, `Stationpedia`
pages and `Human.LocalHuman`, and the shell only patches, logs, and
forwards. Prefab checks stay out of the tests.
(You cannot construct `Thing`, `Human` and friends outside the game — they
are `MonoBehaviour`s.)

This follows the same shape as `D:\Projects\valheim-mods` (Odin's Eye for
Valheim): same model/tool-loop core, same tool/command/index layers, same
MSBuild `stationeers.targets` + `local.props` + deploy/package flow.

## Layout

```
global.json            # runs dotnet test in Microsoft.Testing.Platform (MTP) mode
stationeers.targets    # shared MSBuild config for the plugin: references, deploy, packaging
local.props            # machine-specific paths (git-ignored)
local.props.example    # template for local.props
StationeersMods.sln
lib/BepInEx/           # bundled BepInEx 5.4.22 references (compile fallback when the
                       #   game has no BepInEx installed; never shipped)
StationeersAi.Common/
StationeersAi.Core/
Caev/
StationeersAi.Test/
Caev.Test/
StationeersAi.Mod/
  BepInExPlugin.cs   # thin shell: patches, config, forwards lifecycle to SageMod
  BepinExLogger.cs   # Core.Logger over the BepInEx log
StationeersAi.Game/
  SageMod.cs         # owns window, server, catalogues; Load/Unload per level
  SageConfig.cs      # BepInEx config entries
  AiConsoleCommand.cs  # the `ai` console command
```

## Prerequisites

- **Stationeers** installed (`D:\SteamLibrary\steamapps\common\Stationeers`).
- **BepInEx 5 (x64, Mono)** installed, and the game launched at least once —
  either in the game folder or via r2modman. When the game has no BepInEx,
  compilation falls back to the bundled `lib/BepInEx` references
  automatically; the fallback never ships.
- **.NET SDK 10**. `StationeersAi.Core` targets `netstandard2.0`, the specs
  target `net8.0`, and the plugin targets .NET Framework 4.8 (a NuGet package
  supplies reference assemblies, so you do not need Visual Studio).

## Configure paths

Copy `local.props.example` to `local.props` (already done on this machine) and
set `GamePath`, `BepInExPath`, and `PluginDeployDir`. `stationeers.targets`
auto-detects `GamePath` from the Steam registry if `local.props` is absent.

## Build

```bash
dotnet build StationeersMods.sln -c Debug
```

A successful plugin build copies `StationeersAi.Mod.dll`,
`StationeersAi.Game.dll`, **and**
`StationeersAi.Core.dll` into `<PluginDeployDir>\StationeersAi.Mod\`.

To create a distributable zip:

```bash
dotnet build StationeersAi.Mod/StationeersAi.Mod.csproj -c Release -p:PackageOnBuild=true
# -> StationeersAi.Mod/artifacts/StationeersAi.Mod.zip
```

BepInEx cannot reload plugins, so every change means rebuild + restart the
game, then check `BepInEx/LogOutput.log`. BepInEx holds disk writes back
about 2 seconds by default, so fresh lines can lag. For instant writes
(when you chase a crash), set `InstantFlushing = true` under
`[Logging.Disk]` in `BepInEx.cfg`. Sage flushes held lines to disk after
errors and on unload, so crash context still lands without that setting.

## Test

```bash
dotnet test
```

Gherkin `.feature` files under `StationeersAi.Test/Features/` state the
requirements, with step definitions binding them to fakes. The fakes replace
the LLM and game data, so the specs run without the game. TUnit-only specs
(`SearchTests`, `ToolTests`, `AssistantTests`, `FormattingTests`) test the
same behavior in smaller units.

`ArchitectureTests.cs` enforces the core's defining constraint two ways:

- a raw assembly-reference check (catches unused references), and
- an ArchUnitNET rule that `StationeersAi.Core` types do not depend on
  `BepInEx`/`HarmonyLib`/`UnityEngine`/`Unity`/`Assets.Scripts` namespaces.

### Integration tests

Mark tests that hit a real OpenAI-compatible endpoint with
`[Category("Integration")]` and `[Explicit]`, so a normal `dotnet test` never
hits the network. Provide credentials either through environment variables
(`SAGEAI_ENDPOINT`, `SAGEAI_API_KEY`, `SAGEAI_MODEL`) or a git-ignored
`StationeersAi.Test/integration.settings.json` (copy `integration.settings.example.json`),
then run them explicitly:

```bash
dotnet run --project StationeersAi.Test -- --treenode-filter "/*/*/*/*[Category=Integration]"
```

Never commit credentials.

## Toolchain

| Package | Version | Purpose |
|---|---|---|
| TUnit | 1.67.0 | Test runner (Microsoft.Testing.Platform) |
| Shouldly | 4.3.0 | Assertions |
| TngTech.ArchUnitNET | 0.13.4 | Architecture (namespace dependency) rules |
| TngTech.ArchUnitNET.TUnit | 0.13.4 | TUnit integration for ArchUnitNET |
