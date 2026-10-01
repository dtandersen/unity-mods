# Sage agent performance plan

Source: `D:/SteamLibrary/steamapps/common/Stationeers/BepInEx/LogOutput.log`, 3138 lines, 5 chats.

Log shows Sage fixes the heater IC but wastes rounds. Each `@sage` chat takes 7-12 tool rounds to re-learn the base.

## Goal

Cut rounds per chat from ~10 to ~3. Keep answers correct. Let Sage act where safe.

## 1. Give mentions recall

Yes -> important

Facts:
- `MentionResponder.HandleAsync` sends null for history.
- User says "sensor is wired in", "exported", "still 16c", "bought air".
- Sage re-runs `search_devices`, `describe_device`, `read_ic` each time.

Work:
- Keep one `Conversation` per player for mentions in `BepInExPlugin`.
- Send last sum with each new ask. Drop old lines past cap.
- Tests in `MentionRecall.feature` (kept separate, assistant-level): two asks in a row use prior device ids with no re-search.

## 2. Fix `search_devices` cut

YES

Facts:
- `ToolLimits.MaxResults = 10`.
- Blank search lists APC, battery, cables. Never shows heater, housing, sensor.
- Log shows `query=e`, `query=a`, `query=o`, `query=i`, `query=t`, all return cables.

Work:
- Sort logic devices first.
- Raise cap to 30 when `logic=true`.
- Add `offset`. Return `total` in text.
- Say cap in tool text: "gives first 10, use network/logic to cut".
- Files: `SearchDevicesTool`, `StationeersDeviceCatalog`.
- Tests in `DeviceSearch.feature` (combined).

## 3. Trim `describe_device`

YES

Facts:
- Gas sensor dump is ~40 rows, most are 0 ratios.
- Same sensor read 4-5 times per chat. Temp and pressure get lost.

Work:
- Drop zero values by default.
- Show On, Power, Temp, Pressure, Error first.
- Keep full list behind `full=true`.
- Files: `StationeersDeviceInspector`, `DescribeDeviceTool`.
- Tests in `DescribeDevice.feature` (combined).

## 4. Fix dead errors

YES

Facts:
- `describe_device(device=Gas Sensor)` fails with "No single device matches". Gives no list.
- `search_inventory` with blank query fails. Agent then tries canister, tank, gas, a, e, Iron, Steel.
- `read_ic(computer_name=1189)` fails. 1189 is a thing id, not a label.

Work:
- On multi-match, return match table so user can pick.
- Allow blank inventory query. It lists all worn and held.
- Merge `computer_name` and `computer_thing` to one `computer` that takes label or id.
- Files: `ThingMatching`, `SearchInventoryTool`, `ReadIcTool`, `WriteIcTool`.
- Tests in `DescribeDevice.feature`, `InventoryTools.feature`, `ReadIc.feature`, `WriteIc.feature` (combined).

## 5. Stop full `mips_reference` dump

YES. return only names instead of full text.

Facts:
- Bare call returns ~100 lines. Log shows it twice.

Work:
- Forbid blank call in text: "name one, never call blank".
- On blank, return names only.
- File: `MipsReferenceTool`.
- Tests in `MipsReference.feature` (combined).

## 6. Make `write_ic` flow clear

NO.

Facts:
- Tool writes to Computer #1189 editor, not Housing #1206 chip.
- Log shows hash chase: `8758f623` vs `5ec9a894`, LineNumber 30.

Work:
- Say in tool text: "user must press Export, chip hash must then match".
- Or add `export_ic` step.
- File: `WriteIcTool`.
- Add `WriteExport.feature`.

## 7. Let Sage act

NO.
BUT... we could consider letting sage press EXPORT.

Facts:
- Vents Off, door Open, room 3.3 kPa. Heater needs >6.3 kPa. Sage can only tell user to click.

Work:
- Add `set_device` tool that sets one logic value. Guard with config flag, default off.
- New `SetDeviceTool`, command, fake, docs row.
- Add `SetDevice.feature`.

## 8. Add short prompt notes

NO... for now.

Now prompt says: "Use tools... Keep answers short". It lacks game facts.

Work:
- Add 5 lines to `systemPrompt` in `BepInExPlugin`:
  - `lb/sb` see only the housing net.
  - `sb` needs a register, not `1`.
  - Heater needs >6.3 kPa.
  - Check chip hash equals editor hash after Export.
  - Batch free searches in one block.

## Order

1. Recall, search cap, trim describe.
2. Error texts, one `computer` arg, block bare `mips_reference`.
3. Prompt notes, `set_device` behind flag.

## Checks

- `dotnet test`
- New Reqnroll features pass.
- Live check: ask, wire sensor, export, heat — rounds drop, no dead calls.
