"""Simple telemetry generator for local dashboard demo.
Run together with backend:
  python -m uvicorn python_backend.app:app --host 0.0.0.0 --port 8000
  python python_backend/simulator.py
"""

from __future__ import annotations

import math
import time

import requests

URL = "http://127.0.0.1:8000/telemetry"


def make_payload(t: float) -> dict:
    wipe = min(1.0, t / 200)
    paint = min(1.0, t / 260)
    return {
        "robotId": "WR-001",
        "simTime": t,
        "worldPosition": {"x": math.sin(t / 10), "y": math.cos(t / 12), "z": 0.0},
        "worldEuler": {"x": 0.0, "y": 0.0, "z": (t * 9) % 360},
        "speed": 0.25,
        "spraying": int(t) % 2 == 0,
        "wipeCoverage01": wipe,
        "paintCoverage01": paint,
        "wipedAreaM2": 6.0 * wipe,
        "paintedAreaM2": 6.0 * paint,
        "battery01": max(0.0, 1 - t / 1200),
        "tank01": max(0.0, 1 - t / 800),
        "status": "RUNNING",
    }


def main() -> None:
    start = time.time()
    while True:
        sim_t = time.time() - start
        requests.post(URL, json=make_payload(sim_t), timeout=1)
        time.sleep(0.1)


if __name__ == "__main__":
    main()
