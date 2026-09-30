# Stationeers Mods

BepInEx 5 mod scaffolding for Stationeers, mirroring `D:\Projects\valheim-mods`.
Stationeers lives at `D:\SteamLibrary\steamapps\common\Stationeers`
(`rocketstation.exe` + `rocketstation_Data/Managed/Assembly-CSharp.dll`).

## Layout

```
global.json            # opts dotnet test into Microsoft.Testing.Platform (MTP) mode
stationeers.targets    # shared MSBuild config for plugins: refs + deploy/packaging
local.props            # machine-specific paths (git-ignored, pre-filled for this PC)
local.props.example    # template for local.props
StationeersMods.sln
Example.Mod/           # smoke-test plugin (replace with your real mod)
  BepInExPlugin.cs
  manifest.json        # Thunderstore/r2modman metadata for packaging
```

`stationeers.targets` is the Stationeers equivalent of `valheim.targets`:
same `net48` + deploy-to-`BepInEx/plugins` + zip-package flow, but referencing
`Assembly-CSharp` / `rocketstation_Data/Managed` and auto-detecting the game via
the Steam registry (`Steam App 544550`). There is no Jotunn equivalent, so that
section was dropped.

## Prerequisites

- **Stationeers** installed (this machine: `D:\SteamLibrary\steamapps\common\Stationeers`).
- **BepInEx 5 (x64, Mono)** installed/launched at least once — **not yet installed
  for Stationeers on this machine**. Either:
  - install via **r2modman** (create a Stationeers profile — none exists yet), or
  - manually unzip BepInEx 5 x64 into the game folder so
    `Stationeers/BepInEx/core/BepInEx.dll` exists.
- **.NET SDK 10**.

## Configure paths

`local.props` is already set to `D:\SteamLibrary\steamapps\common\Stationeers`
with `BepInExPath` defaulting to `$(GamePath)/BepInEx`. If you use r2modman,
point `BepInExPath`/`PluginDeployDir` at
`%APPDATA%\r2modmanPlus-local\Stationeers\profiles\<profile>\BepInEx[...\plugins]`
(see `local.props.example`). `stationeers.targets` auto-detects `GamePath` from
the Steam registry if `local.props` is absent.

## Build

```bash
dotnet build StationeersMods.sln -c Debug
```

A successful plugin build copies the DLL (and pdb) into
`<PluginDeployDir>\Example.Mod\`. **This currently fails until BepInEx is
installed** (missing `BepInEx.dll`/`0Harmony.dll`) — install it first, then
rebuild.

To create a distributable zip:

```bash
dotnet build Example.Mod/Example.Mod.csproj -c Release -p:PackageOnBuild=true
# -> Example.Mod/artifacts/Example.Mod.zip
```

## Next steps

- Install BepInEx 5 for Stationeers (above), then `dotnet build` to verify.
- Copy/rename `Example.Mod` to your real mod name, or add `MyMod.Core` /
  `MyMod.Test` projects mirroring the `OdinsAi.Core` (netstandard2.0, Unity-free)
  + `OdinsAi.Test` (net8.0) split if you want testable game-free logic.
