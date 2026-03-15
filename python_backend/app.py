from __future__ import annotations

import asyncio
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

from fastapi import FastAPI, HTTPException, WebSocket, WebSocketDisconnect
from fastapi.responses import HTMLResponse
from pydantic import BaseModel, Field

BASE_DIR = Path(__file__).resolve().parent
TEMPLATE_PATH = BASE_DIR / "templates" / "dashboard.html"

app = FastAPI(title="Window Robot Twin Backend", version="1.1.0")


class Telemetry(BaseModel):
    robotId: str = Field(min_length=1)
    simTime: float = Field(ge=0)
    worldPosition: dict[str, Any] | list[float] | None = None
    worldEuler: dict[str, Any] | list[float] | None = None
    speed: float = Field(ge=0)
    spraying: bool
    wipeCoverage01: float = Field(ge=0, le=1)
    paintCoverage01: float = Field(ge=0, le=1)
    wipedAreaM2: float = Field(ge=0)
    paintedAreaM2: float = Field(ge=0)
    battery01: float = Field(ge=0, le=1)
    tank01: float = Field(ge=0, le=1)
    status: str


class Command(BaseModel):
    robotId: str
    action: str
    payload: dict[str, Any] = Field(default_factory=dict)


latest_state: dict[str, Any] = {
    "updatedAt": datetime.now(timezone.utc).isoformat(),
    "telemetry": None,
    "lastCommand": None,
}
ws_clients: set[WebSocket] = set()


@app.get("/")
async def index() -> HTMLResponse:
    if not TEMPLATE_PATH.exists():
        raise HTTPException(status_code=500, detail="dashboard template not found")
    return HTMLResponse(TEMPLATE_PATH.read_text(encoding="utf-8"))


@app.get("/health")
async def health() -> dict[str, str]:
    return {"status": "ok"}


@app.post("/telemetry")
async def push_telemetry(payload: Telemetry) -> dict[str, str]:
    latest_state["updatedAt"] = datetime.now(timezone.utc).isoformat()
    latest_state["telemetry"] = payload.model_dump()
    await broadcast_state()
    return {"message": "ok"}


@app.post("/command")
async def push_command(cmd: Command) -> dict[str, str]:
    latest_state["updatedAt"] = datetime.now(timezone.utc).isoformat()
    latest_state["lastCommand"] = cmd.model_dump()
    await broadcast_state()
    return {"message": "queued"}


@app.get("/telemetry/latest")
async def get_latest() -> dict[str, Any]:
    return latest_state


@app.websocket("/ws")
async def ws_endpoint(ws: WebSocket) -> None:
    await ws.accept()
    ws_clients.add(ws)
    try:
        await ws.send_json(latest_state)
        while True:
            await asyncio.sleep(30)
    except WebSocketDisconnect:
        pass
    finally:
        ws_clients.discard(ws)


async def broadcast_state() -> None:
    dead: list[WebSocket] = []
    for ws in ws_clients:
        try:
            await ws.send_json(latest_state)
        except Exception:
            dead.append(ws)

    for ws in dead:
        ws_clients.discard(ws)
