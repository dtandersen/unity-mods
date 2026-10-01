# Station Automation & Guidance Engine (SAGE)

Sage is an assistant for Stationeers, shipped as a BepInEx 5 plugin. Ask it
questions and it looks up live game data — items, structures, manufacturing
recipes, Stationpedia pages, your inventory, and your position — before
answering.

## Use

Press Enter to open chat and mention SAGE or press F3 to use the console (`ai <prompt>`):

```
@sage how do I make cable coil?
@sage what does the autolathe need for steel sheets?
```

SAGE posts the answer to chat — on a server, every player sees it. Two settings control mention handling:
`General/RespondToMentions` (default on) and `General/MentionTrigger`
(default `@sage`).

For longer sessions with history, use the console (default key F3, rebindable
in settings):

```
ai how do I make cable coil?
ai clear     # drop conversation history
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
| `get_status` | Your position and world |

## Development

Building, testing, and architecture notes live in [development.md](development.md).

## License

Public domain.
