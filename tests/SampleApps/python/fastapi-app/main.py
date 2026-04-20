import asyncio
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.responses import StreamingResponse
from datetime import datetime
from pydantic import BaseModel

app = FastAPI()


class Item(BaseModel):
    name: str
    price: float
    quantity: int = 1


@app.get("/")
def read_root():
    return {"message": "Hello FastAPI!", "timestamp": str(datetime.now())}


@app.get("/async")
async def async_root():
    await asyncio.sleep(0.1)
    return {"message": "Hello from async!", "async": True}


@app.get("/items/{item_id}")
def get_item(item_id: int, q: str = None):
    return {"item_id": item_id, "q": q}


@app.post("/items")
async def create_item(item: Item):
    total = item.price * item.quantity
    return {"name": item.name, "total": total}


@app.get("/stream")
async def stream_response():
    async def generate():
        for i in range(5):
            yield f"chunk {i}\n"
            await asyncio.sleep(0.05)
    return StreamingResponse(generate(), media_type="text/plain")


@app.websocket("/ws")
async def websocket_endpoint(ws: WebSocket):
    await ws.accept()
    try:
        while True:
            data = await ws.receive_text()
            await ws.send_text(f"echo: {data}")
    except WebSocketDisconnect:
        pass
