# Sage (StationeersAi)

An LLM assistant for Stationeers, delivered as a BepInEx 5 plugin. Ask questions
from the in-game console (`ai <question>`) and the assistant looks up live game
data — items, structures, manufacturing recipes, Stationpedia pages, your
inventory, and your position — before answering.

Named Sage, after the Stationeers engineer who reads the manual. 🧠

## Projects

```
StationeersAi.Core/   netstandard2.0   Unity-free logic (models, tools, LLM loop, parsing)
StationeersAi.Test/   net8.0           TUnit + Shouldly + ArchUnitNET specs
StationeersAi.Mod/    net48            BepInEx plugin: Unity/HTTP/game-data adapter
```

The split is deliberate: **`StationeersAi.Core` must not touch Unity or
Stationeers types** so it can be specified and tested from
`StationeersAi.Test`. The plugin is a thin adapter that snapshots
`Prefab.AllPrefabs`, the `ElectronicReader` recipe tables, `Stationpedia`
pages and `Human.LocalHuman`, calls the LLM over `UnityWebRequest`, and
prints results. (`Thing`, `Human` and friends are `MonoBehaviour`s and cannot
be constructed outside the game.)

Mirrors `D:\Projects\valheim-mods` (Odin's Eye for Valheim): same
model/tool-loop core, same tool/command/index layering, same MSBuild
`stationeers.targets` + `local.props` + deploy/package flow.

## Layout

```
global.json            # opts dotnet test into Microsoft.Testing.Platform (MTP) mode
stationeers.targets    # shared MSBuild config for the plugin: refs + deploy/packaging
local.props            # machine-specific paths (git-ignored)
local.props.example    # template for local.props
StationeersMods.sln
lib/BepInEx/           # vendored BepInEx 5.4.22 compile refs (fallback when the
                       #   game has no BepInEx installed; never shipped)
StationeersAi.Core/
StationeersAi.Test/
StationeersAi.Mod/
  BepInExPlugin.cs
  AiConsoleCommand.cs  # the `ai` console command
```

## Prerequisites

- **Stationeers** installed (`D:\SteamLibrary\steamapps\common\Stationeers`).
- **BepInEx 5 (x64, Mono)** installed/launched at least once — either in the
  game folder or via r2modman. For *compiling* only, the vendored
  `lib/BepInEx` fallback is used automatically when BepInEx is absent.
- **.NET SDK 10**. `StationeersAi.Core` targets `netstandard2.0`, the specs
  target `net8.0`, and the plugin targets .NET Framework 4.8 (reference
  assemblies are supplied by a NuGet package, so Visual Studio is not required).

## Configure paths

Copy `local.props.example` to `local.props` (already done on this machine) and
set `GamePath`, `BepInExPath`, and `PluginDeployDir`. `stationeers.targets`
auto-detects `GamePath` from the Steam registry if `local.props` is absent.

## Build

```bash
dotnet build StationeersMods.sln -c Debug
```

A successful plugin build copies `StationeersAi.Mod.dll` **and**
`StationeersAi.Core.dll` into `<PluginDeployDir>\StationeersAi.Mod\`.

To create a distributable zip:

```bash
dotnet build StationeersAi.Mod/StationeersAi.Mod.csproj -c Release -p:PackageOnBuild=true
# -> StationeersAi.Mod/artifacts/StationeersAi.Mod.zip
```

## Test

```bash
dotnet test
```

Requirements are written as Gherkin `.feature` files under
`StationeersAi.Test/Features/` and bound to step definitions. The LLM and game
data are replaced with fakes so the specs run without the game. TUnit-only
specs (`SearchTests`, `ToolTests`, `AssistantTests`, `FormattingTests`) cover
the same ground at a finer grain.

`ArchitectureTests.cs` enforces the core's defining constraint two ways:

- a raw assembly-reference check (catches unused references), and
- an ArchUnitNET rule that `StationeersAi.Core` types do not depend on
  `BepInEx`/`HarmonyLib`/`UnityEngine`/`Unity`/`Assets.Scripts` namespaces.

### Integration tests

Tests that talk to a real OpenAI-compatible endpoint are marked with
`[Category("Integration")]` and `[Explicit]`, so a normal `dotnet test` never hits the
network. Provide credentials either through environment variables
(`SAGEAI_ENDPOINT`, `SAGEAI_API_KEY`, `SAGEAI_MODEL`) or a git-ignored
`StationeersAi.Test/integration.settings.json` (copy `integration.settings.example.json`),
then run them explicitly:

```bash
dotnet run --project StationeersAi.Test -- --treenode-filter "/*/*/*/*[Category=Integration]"
```

Credentials are never committed.

## Use

Press Enter to open chat and mention Sage, or use the console (`ai`):

```
@sage how do I make cable coil?
@sage what does the autolathe need for steel sheets?
```

The answer is posted to chat as Sage — on a server, every player sees it.
Chat answers are plain text (no Markdown). Mention handling is controlled by
`General/RespondToMentions` (default on) and `General/MentionTrigger`
(default `@sage`).

From the console (Stationeers console key, e.g. F1) for longer sessions with
history:

```
ai how do I make cable coil?
ai clear     # drop conversation history
ai compact   # summarise history into one message
```

Set `AI/ApiKey` (and optionally `Endpoint`/`Model`) in the BepInEx config
(`BepInEx/config/davidandersen.stationeersai.cfg`) first — any
OpenAI-compatible endpoint works.

The assistant can call these tools:

| Tool | Data |
|---|---|
| `search_items` | Items by name (from the prefab catalogue) |
| `search_structures` | Buildable structures by name |
| `search_recipes` | Manufacturing recipes with station + ingredients |
| `search_stationpedia` / `read_stationpedia` | In-game encyclopedia pages |
| `search_inventory` | What you are carrying and where it is worn |
| `get_status` | Your position and world |

## Toolchain

| Package | Version | Purpose |
|---|---|---|
| TUnit | 1.67.0 | Test runner (Microsoft.Testing.Platform) |
| Shouldly | 4.3.0 | Assertions |
| TngTech.ArchUnitNET | 0.13.4 | Architecture (namespace dependency) rules |
| TngTech.ArchUnitNET.TUnit | 0.13.4 | TUnit integration for ArchUnitNET |

## License

Public domain.
