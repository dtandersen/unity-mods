# Station Automation & Guidance Engine (SAGE)

Sage is an assistant for Stationeers, shipped as a BepInEx 5 plugin. Ask it
questions and it looks up live game data — items, structures, manufacturing
recipes, Stationpedia pages, your inventory, and your position — before
answering.

## Install

Pick one path (the same zip runs either way — it holds the DLLs
plus `About/About.xml`; BepInEx ignores the `About` folder and LaunchPad
uses it):

- **BepInEx:** extract `sage-<version>.zip` (e.g.
  `sage-0.1.0.zip`) into `steamapps/common/Stationeers/BepInEx/plugins/Sage/`.
  Needs BepInEx 5 only.
- **LaunchPad Local mod:** extract `sage-<version>.zip` (e.g.
  `sage-0.1.0.zip`) into `Documents/My Games/Stationeers/mods/Sage/`, Needs BepInEx 5
  plus the StationeersLaunchPad plugin. Do not drop the `.zip` in
  unopened — LaunchPad only finds mods by their `About/About.xml` file.

Do not install both at once.

## Usage

There are two ways to interact with SAGE. You can mention `@sage <prompt>` in a chat message or use `ai <prompt>` in the Stationeers console.

```
@sage how do I make cable coil?
ai what does the autolathe need for steel sheets?
```

SAGE answers every question in chat — on a server, every player sees it — and
shows the same answer in the Sage window. That includes questions you type in the
window or the console, not just `@sage` mentions. Two settings control mention
handling: `General/RespondToMentions` (default on) and `General/MentionTrigger`
(default `@sage`).

For longer sessions with history, use the console (default key F3, rebindable
in settings):

```
ai how do I make cable coil?
ai reset     # drop conversation history
ai compact   # summarise history into one message
```

First set `AI/ApiKey` (and optionally `Endpoint`/`Model`) in the BepInEx
config (`BepInEx/config/dtandersen.sage.cfg`) — any
OpenAI-compatible endpoint works.

Sage can call these tools:

| Tool | Data |
|---|---|
| `search_items` | Items by name (from the prefab catalogue) |
| `search_structures` | Buildable structures by name, with build states |
| `search_recipes` | Manufacturing recipes with station + ingredients |
| `search_stationpedia` / `read_stationpedia` | In-game encyclopedia pages |
| `search_inventory` | What you are carrying and where it is worn |
| `describe_player` | Your position and world |
| `get_world` | World, orbit, sunlight, and outside air |
| `write_ic` | Place MIPS source in a computer's editor, after naming the selected chip's hash |
| `read_ic` | Read a computer's editor text, by thing id |
| `export_ic` | Burn editor text to a housing chip, hash-verified, by thing id |
| `import_ic` | Pull the chip under the dropdown into a computer's editor |
| `select_ic` | Point a computer's dropdown at one housing on its network |
| `read_slots` | List what one thing holds, slot by slot |
| `list_instructions` | IC10 instruction usage and descriptions |
| `search_devices` | Live devices: labels, types, networks |
| `describe_device` | One device: ports, settings, slots, build state |

Log file:

`steamapps/common/Stationeers/BepInEx/LogOutput.log`

## HTTP server

Sage can serve game data over HTTP for outside tools such as Pi. Set `Server/Enabled` to true and restart. Set `Server/Port` (default 8080) and `Server/Token`. The server binds 127.0.0.1 only, so only this machine can call; a set token guards all calls. It never sets device values; it reads state and burns IC only through `export_ic`.

```
GET /sage/status
GET /sage/devices?q=vent
```

Each answer is full JSON. Specs live in `Stationeers/Sage.Test/Features/Server/SageServer.feature`.

## Development

Building, testing, and architecture notes live in [development.md](development.md).

## Credits

StationeersWebApi showed that a live game can serve HTTP. sunsetfi and robophred built it; dtandersen keeps a fork. It maps devices, logic, things, and air to REST routes and moves game reads to the main thread. Sage reuses these ideas, not its code.

## License

[MIT](license.md). Use at your own risk.
