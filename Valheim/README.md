# Odin's Eye (OdinsAi)

An LLM assistant for Valheim, delivered as a BepInEx 5 plugin, plus an in-game
tool that can answer questions about recipes, food stats, and the in-game
compendium of lore and Munin's hints.

Named after Odin's eye — the all-seeing assistant. 👁️

## Projects

```
OdinsAi.Core/   netstandard2.0   Unity-free logic (models, tools, LLM loop, parsing)
OdinsAi.Test/   net8.0           TUnit + Reqnroll + Shouldly + ArchUnitNET specs
OdinsAi.Mod/    net48            BepInEx plugin: Unity/HTTP/Harmony adapter
```

The split is deliberate: **`OdinsAi.Core` must not touch Unity or Valheim types**
so it can be specified and tested from `OdinsAi.Test`. The plugin is a thin adapter
that reads `ObjectDB`/`SharedData`, calls the LLM over `UnityWebRequest`, and renders
results. (`ObjectDB`, `Recipe`, and `ItemDrop` are `MonoBehaviour`/`ScriptableObject`
and cannot be constructed outside the game.)

## Layout

```
global.json            # opts dotnet test into Microsoft.Testing.Platform (MTP) mode
valheim.targets        # shared MSBuild config for the plugin: refs + deploy/packaging
local.props            # machine-specific paths (git-ignored)
local.props.example    # template for local.props
ValheimMods.sln
OdinsAi.Core/
OdinsAi.Test/
  reqnroll.json        # Reqnroll configuration
  ArchitectureTests.cs # core must not depend on BepInEx/Valheim/Unity
  Features/            # Gherkin feature files + step definitions
OdinsAi.Mod/
  BepInExPlugin.cs
```

## Prerequisites

- **Valheim** installed.
- **BepInEx 5 (x64, Mono)** installed/launched at least once. With r2modman this
  lives at `%APPDATA%\r2modmanPlus-local\Valheim\profiles\<profile>\BepInEx`.
- **.NET SDK 10** (required for the `dotnet test` MTP mode configured in `global.json`).
  `OdinsAi.Core` targets `netstandard2.0`, the specs target `net8.0`, and the plugin
  targets .NET Framework 4.8 (reference assemblies are supplied by a NuGet package,
  so Visual Studio is not required).

## Configure paths

Copy `local.props.example` to `local.props` and set `GamePath`, `BepInExPath`, and
`PluginDeployDir`. `valheim.targets` auto-detects `GamePath` from the Steam registry
if `local.props` is absent.

## Build

```bash
dotnet build ValheimMods.sln -c Debug
```

A successful plugin build copies `OdinsAi.Mod.dll` **and** `OdinsAi.Core.dll` into
`<PluginDeployDir>\OdinsAi.Mod\`.

To create a distributable zip:

```bash
dotnet build OdinsAi.Mod/OdinsAi.Mod.csproj -c Release -p:PackageOnBuild=true
# -> OdinsAi.Mod/artifacts/OdinsAi.Mod.zip
```

## Test

```bash
dotnet test
```

Requirements are written as Gherkin `.feature` files under `OdinsAi.Test/Features/`
and bound to step definitions. The LLM and game data are replaced with fakes so the
specs run without the game.

`ArchitectureTests.cs` enforces the core's defining constraint two ways:

- a raw assembly-reference check (catches unused references), and
- an ArchUnitNET rule that `OdinsAi.Core` types do not depend on
  `BepInEx`/`HarmonyLib`/`UnityEngine`/`Unity`/`Jotunn`/`Valheim` namespaces.

### Integration tests

Tests that talk to a real OpenAI-compatible endpoint are marked with
`[Category("Integration")]` and `[Explicit]`, so a normal `dotnet test` never hits the
network. Provide credentials either through environment variables
(`ODINSAI_ENDPOINT`, `ODINSAI_API_KEY`, `ODINSAI_MODEL`) or a git-ignored
`OdinsAi.Test/integration.settings.json` (copy `integration.settings.example.json`),
then run them explicitly:

```bash
dotnet run --project OdinsAi.Test -- --treenode-filter "/*/*/*/*[Category=Integration]"
```

Credentials are never committed.

## Toolchain

| Package | Version | Purpose |
|---|---|---|
| Reqnroll.TUnit | 3.3.4 | Gherkin feature specs (BDD) |
| TUnit | 1.67.0 | Test runner (Microsoft.Testing.Platform) |
| Shouldly | 4.3.0 | Assertions in step definitions |
| TngTech.ArchUnitNET | 0.13.4 | Architecture (namespace dependency) rules |
| TngTech.ArchUnitNET.TUnit | 0.13.4 | TUnit integration for ArchUnitNET |

## License

Public domain.