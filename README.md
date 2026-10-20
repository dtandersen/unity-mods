# Unity Mods

Home for my Unity game mods. Each game lives in its own folder with its own
solution; shared libraries sit at the root.

- **Stationeers — [SAGE](Stationeers/README.md):** an assistant shipped as a
  BepInEx 5 plugin. Ask it questions and it looks up live game data — items,
  structures, recipes, Stationpedia pages, inventory, and position — before
  answering. Builds to `sage-<version>.zip` for BepInEx or LaunchPad Local.
  Solution: `Stationeers/Sage.sln`.
- **Valheim — [Odin's Eye](Valheim/README.md):** an LLM assistant delivered as
  a BepInEx 5 plugin, answering questions about recipes, food stats, and the
  in-game compendium of lore and Munin's hints. Solution:
  `Valheim/OdinsAi.sln`.

## Shared

```
Caev/            netstandard2.0   Agent loop, tools, chat transports
Common/          netstandard2.0   Shared bits used on both sides (trace log)
UnityStubs/      net48 fakes      Minimal UnityEngine stand-ins (CI only)
global.json                  # opts dotnet test into Microsoft.Testing.Platform (MTP) mode
stationeers.targets          # shared MSBuild config for the Stationeers plugin
valheim.targets              # shared MSBuild config for the Valheim plugin
local.props                  # machine-specific paths (git-ignored)
local.props.example          # template for local.props
```

Each mod keeps its game adapter thin: game-specific logic stays testable from
its test project, and the plugin only bridges Unity/HTTP/Harmony.

## Build

```bash
# Stationeers (no game install needed: stub DLLs stand in for game assemblies)
dotnet test Stationeers/Sage.sln -c Release -p:DeployOnBuild=false -p:DeployLocalOnBuild=false

# Valheim (needs the game or dedicated-server assemblies for the Mod project)
dotnet test Valheim/OdinsAi.sln -c Release
```

Copy `local.props.example` to `local.props` and set `GamePath` to the game
for the solution you are building. Per-game details live in
[Stationeers](Stationeers/README.md) and [Valheim](Valheim/README.md).

## License

Sage: [MIT](license.md). Odin's Eye: public domain.
