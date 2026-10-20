# Station Automation & Guidance Engine (SAGE) development

## Projects

```
StationeersAi.Common/ netstandard2.0   Shared bits used on both sides (trace log)
StationeersAi.Core/   netstandard2.0   Unity-free game data (models, tools, commands, chat, window)
Caev/               netstandard2.0   Generic agent engine (loop, clients, messages, tool ports)
StationeersAi.Test/   net8.0           TUnit + Shouldly + ArchUnitNET specs (game, chat, window)
Caev.Test/          net8.0           Engine specs: loop, events, OpenAI wire (stub tools, fake server)
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
  SageMod.cs         # builds the agent on the first level load; owns window, server, chat hook
  SageConfig.cs      # BepInEx config entries
  AiConsoleCommand.cs  # the `ai` console command
  View/ChatView.cs   # posts lines to game chat on the main thread
```

## Turn lifecycle

`SageMod.Load` builds one agent for the mod and keeps it across levels:
`SageAgentFactory.BuildGateway()` makes the `AgentGateway` (a
`CaevAgentGateway` over the engine's `Agent`), then the chat view and the
presenter. Every turn asks that one gateway and passes a `TurnContext` (prompt,
history, system prompt, tool-round limit), so config edits apply to the next turn
without rebuilding anything.

The gateway owns one `AgentEvents`. `SagePresenter` (`Core/Presentation/`) is the
only object that starts a turn, so its busy flag is the single gate across window,
console, and chat: the second question while one runs is refused and logged. It
drives two passive views — `ISageView` for the window, `IChatView.Post(name, text)`
for chat — both in `Core/View/`. Reasoning and tool traffic go to the window per
their options; every answer goes to the window and to chat, in plain text, with no
name in the text because the game shows the name it posts under.

Whether a chat line asks something is `ChatRouting`'s call (`Core/Chat/`), from the
poster's name, the trigger, the answering switch, and whether anyone else is in the
game. The mod reads those four and hands the line over; `NetworkManager` and
`ChatMessage` stay in `StationeersAi.Game`.

## Prerequisites

- **Stationeers** installed (`D:\SteamLibrary\steamapps\common\Stationeers`).
- **BepInEx 5 (x64, Mono)** installed, and the game launched at least once —
  in the game folder. When the game has no BepInEx,
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
# -> StationeersAi.Mod/artifacts/sage-<version>.zip  (DLLs + About/About.xml at the root)
```

## Install paths (BepInEx or LaunchPad Local)

The same DLLs run either way — LaunchPad finds any `BaseUnityPlugin` under
the mod folder, so no code change is needed for the second path. Install
one of them, not both.

- **BepInEx plugin:** extract the zip into
  `<Game>/BepInEx/plugins/Sage/` (this is what `DeployOnBuild` does, minus the
  `About` folder). Needs BepInEx 5 only.
- **LaunchPad Local mod:** extract the zip into
  `Documents/My Games/Stationeers/mods/Sage/`, so it holds
  `About/About.xml` plus the DLLs. Do not drop the `.zip` in unopened —
  LaunchPad only finds mods by their `About/About.xml` file. Needs
  BepInEx 5 plus the StationeersLaunchPad plugin. Pass `-p:DeployLocalOnBuild=true` to copy
  there on build (override the folder with `LocalModsPath` /
  `LocalModFolderName` in `local.props`).

`StationeersAi.Mod/About/About.xml` carries the LaunchPad metadata
(`ModID` `dtandersen.sage`, version `0.1.0`). Bump it with `ModVersion` in
`BepInExPlugin.cs` and `Version` in `StationeersAi.Mod.csproj` on release.

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

## Live objects vs definitions

- `/things` lists loose things (ore, ingots, plants, dropped items).
  `/things/{thing}` reads one live thing of either kind.
- `/structures` lists built structures (machines, pipes, walls).
  `/structures/{structure}` reads one built structure in full.
- `/stationpedia` defines what things are: how to make them
  (`HowToBuild`), how to place them stage by stage (`BuildSteps`),
  what uses them (`UsedIn`), and how they burn (`Combustion`).

## Data dictionary

- Id: ReferenceId of a thing instance (0 when the screw is empty)
- Kind: `structure` for built things, `thing` for loose ones
- PrefabName: name of the Unity prefab (e.g. StructureFurnace)
- Name: localized display name of a thing
- RoomId: room the thing stands in (0 outside rooms)
- Connections: every port on a built thing, each with Kind (power, data,
  pipe, chute), Index within that kind, NetworkId joined, and Port for
  pipes (input, input2, output, output2 — output2 often carries liquids,
  e.g. the furnace liquid out). A battery reads power 0, power 1, data 0;
  a furnace reads pipe input, pipe output, pipe output2. Empty when the
  thing joins nothing; one wire can carry both power and data when entries
  share a network id.
- position: x, y, z coordinates of a thing in meters
- BuildState: one unfinished build stage — its number and requirements (tool held, materials with quantities)
- BuildSteps: placement plan from Stationpedia, one line per stage
- RecipeId: prefab name of what the recipe makes (the station names the machine)
- Quantity: how much is there — game text on carried stacks (x5, 75%), a number on build entries (blank means a tool you hold)
