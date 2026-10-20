# Vendored Jotunn (compile-time reference only)

- File: `Jotunn.dll` (516,608 bytes)
- Source: https://thunderstore.io/package/download/ValheimModding/Jotunn/2.30.0/
- Upstream: https://github.com/ValheimModding/Jotunn
- Purpose: referenced with `Private=False`, so it is never shipped in the
  mod zip. Used when Jotunn isn't installed alongside the game (e.g. CI);
  a copy installed via the mod manager takes precedence (see
  `valheim.targets`).
- License: see upstream. When updating, keep this note and the version in
  `OdinsAi.Mod/manifest.json` in sync.
