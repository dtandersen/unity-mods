# Vendored compile-time references (BepInEx 5.4.22, x64 Mono).

`Stationeers/stationeers.targets` prefers the BepInEx copy installed alongside the game
(`$(BepInExPath)\core`) and only falls back to these DLLs so the plugin
compiles on machines (and CI) without BepInEx installed. They are referenced
with `Private=False` and are never shipped in the mod zip.

Source: https://github.com/BepInEx/BepInEx/releases/tag/v5.4.22
(`BepInEx_x64_5.4.22.0.zip`, `BepInEx/core/BepInEx.dll` + `0Harmony.dll`).
