# Integration tests (live game)

These run against a running game with the Sage mod loaded. They are not
part of `dotnet test`: they need the HTTP server up on loopback.

## Run

```bash
# Game running, world loaded, Sage server on (Server/Enabled, default port 8090 here)
python ic-burn.py http://127.0.0.1:8090
```

Needs `python3` (stdlib only), and on the used computer's network: one computer
with an editor plus two IC housings. The script discovers them, so no
save-specific ids are baked in.

## What ic-burn.sh checks

1. API answers (`GET /sage/`).
2. Finds two housings and one computer on the same network.
3. Burns program A (light on) to housing 1, reads the chip back, compares
   text and hash.
4. Burns program B (light off) to housing 2, reads back, compares.
5. Confirms the two housings hold different code.

A `Written:true` answer already means the server hash-checked the burn;
the read-back compare proves it a second time.
