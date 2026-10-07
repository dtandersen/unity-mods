# Station Automation & Guidance Engine (SAGE)

Sage is an assistant for Stationeers, shipped as a BepInEx 5 plugin. Ask it
questions and it looks up live game data — items, structures, manufacturing
recipes, Stationpedia pages, your inventory, and your position — before
answering.

## Usage

There are two ways to interact with SAGE. You can mention `@sage <prompt>` in a chat message or use `ai <prompt>` in the Stationeers console.

```
@sage how do I make cable coil?
ai what does the autolathe need for steel sheets?
```

SAGE posts the answer to chat — on a server, every player sees it. Two settings control mention handling:
`General/RespondToMentions` (default on) and `General/MentionTrigger`
(default `@sage`).

For longer sessions with history, use the console (default key F3, rebindable
in settings):

```
ai how do I make cable coil?
ai reset     # drop conversation history
ai compact   # summarise history into one message
```

First set `AI/ApiKey` (and optionally `Endpoint`/`Model`) in the BepInEx
config (`BepInEx/config/davidandersen.stationeersai.cfg`) — any
OpenAI-compatible endpoint works.

Sage can call these tools:

| Tool | Data |
|---|---|
| `search_items` | Items by name (from the prefab catalogue) |
| `search_structures` | Buildable structures by name |
| `search_recipes` | Manufacturing recipes with station + ingredients |
| `search_stationpedia` / `read_stationpedia` | In-game encyclopedia pages |
| `search_inventory` | What you are carrying and where it is worn |
| `describe_player` | Your position and world |
| `get_world` | World, orbit, sunlight, and outside air |
| `write_ic` | Place MIPS source in a computer's editor, after naming the pointed chip's hash |
| `read_ic` | Read a computer's editor text, by thing id |
| `export_ic` | Burn editor text to a housing chip, hash-verified, by thing id |
| `list_instructions` | IC10 instruction usage and descriptions |
| `search_devices` | Live devices: labels, types, networks |
| `describe_device` | One device: ports, settings, slots |

Log file:

`steamapps/common/Stationeers/BepInEx/LogOutput.log`

## HTTP server

Sage can serve game data over HTTP for outside tools such as Pi. Set `Server/Enabled` to true and restart. Set `Server/Port` (default 8080) and `Server/Token`. The server binds 127.0.0.1 only, so only this machine can call; a set token guards all calls. It never sets device values; it reads state and burns IC only through `export_ic`.

```
GET /sage/status
GET /sage/devices?q=vent
```

Each answer is full JSON. Specs live in `StationeersAi.Test/Features/Server/SageServer.feature`.

## Development

Building, testing, and architecture notes live in [development.md](development.md).

## Credits

StationeersWebApi showed that a live game can serve HTTP. sunsetfi and robophred built it; dtandersen keeps a fork. It maps devices, logic, things, and air to REST routes and moves game reads to the main thread. Sage reuses these ideas, not its code.

## License

[MIT](license.md). Use at your own risk.
