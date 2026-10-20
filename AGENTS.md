## Product

Sage diagnoses, drafts, and — only when the stationeer agrees — burns code
to chips through `export_ic` with hash verification. It never sets device
values: automating the station defeats IC10, which is the game.

## Testing

Create Reqnroll features instead of TUnit test cases when able.

## Documentation

- Never use a metaphor, simile, or other figure of speech which you are used to seeing in print.
- Never use a long word where a short one will do.
- If it is possible to cut a word out, always cut it out.
- Never use the passive where you can use the active.
- Never use a foreign phrase, a scientific word, or a jargon word if you can think of an everyday English equivalent.

## Before commit

Run `gitleaks detect` and `gitleaks protect --staged`. Fix all leaks before you commit.

## Development

Read development.md for build, test, and layout.

Leave changes uncomitted for review.
