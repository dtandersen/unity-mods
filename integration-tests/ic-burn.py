#!/usr/bin/env python3
"""Live-game IC burn test: two programs onto two housings, hash-verified.

Usage: python ic-burn.py [base-url, default http://127.0.0.1:8090]

Needs a running game with the Sage mod loaded, plus one computer and two
IC housings on the same network. Discovers them, so no save-specific ids.
"""
import hashlib
import json
import sys
import urllib.parse
import urllib.request

BASE = sys.argv[1] if len(sys.argv) > 1 else "http://127.0.0.1:8090"
PASS = 0
FAIL = 0

PROGRAM_A = "# Light on\ndefine LIGHT -1860064656\nmove r0 1\nsb LIGHT On r0"
PROGRAM_B = "# Light off\ndefine LIGHT -1860064656\nmove r0 0\nsb LIGHT On r0"


def sha(text):
    return hashlib.sha256(text.encode()).hexdigest()[:12]


def call(method, path, body=None):
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request(BASE + path, data=data, method=method,
                                 headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=25) as res:
        text = res.read().decode()
        try:
            return res.status, json.loads(text)
        except ValueError:
            return res.status, text


def check(name, got, want):
    global PASS, FAIL
    if got == want:
        PASS += 1
        print("ok:", name)
    else:
        FAIL += 1
        print("FAIL:", name, "got [%s] want [%s]" % (got, want))


print("== api ==")
status, _ = call("GET", "/sage/")
check("index answers", status, 200)

print("== discover ==")
_, devices = call("GET", "/sage/devices?q=")
housings = [d for d in devices if d.get("Type") == "StructureCircuitHousing"]
computers = [d for d in devices if d.get("Type") == "StructureComputer"]
assert len(housings) >= 2 and computers, "need 1 computer + 2 housings"
# Sage addresses devices by thing id; labels are for the printout.
names = {d["Thing"]: d["Name"] for d in devices}
computer, h1, h2 = computers[0]["Thing"], housings[0]["Thing"], housings[1]["Thing"]
print("computer:", computer, names[computer], "| housings:", h1, names[h1], "/", h2, names[h2])


def burn(housing, program):
    status, res = call("PUT", "/sage/computer/%s" % computer,
                       {"sourceCode": program, "device": str(housing), "export": True})
    print(res)
    return status, res


def readback(housing):
    _, res = call("GET", "/sage/circuit-housings/%s" % housing)
    return res.get("SourceCode") or ""


print("== labels are refused ==")
status, res = call("PUT", "/sage/computer/%s" % computer,
                   {"sourceCode": PROGRAM_A, "device": names[h1], "export": True})
check("burn by housing label refused", status, 400)
status, _ = call("GET", "/sage/computer/%s" % urllib.parse.quote(names[computer]))
check("read by computer label refused", status, 400)


print("== A -> %s ==" % h1)
status, res = burn(h1, PROGRAM_A)
check("burn A written", res.get("Written"), True)
check("read-back A matches", sha(readback(h1)), sha(PROGRAM_A))

print("== B -> %s ==" % h2)
status, res = burn(h2, PROGRAM_B)
check("burn B written", res.get("Written"), True)
check("read-back B matches", sha(readback(h2)), sha(PROGRAM_B))

check("housings differ", readback(h1) != readback(h2), True)

print("== %d passed, %d failed ==" % (PASS, FAIL))
sys.exit(1 if FAIL else 0)
