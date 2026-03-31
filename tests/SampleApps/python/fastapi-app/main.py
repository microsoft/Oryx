from fastapi import FastAPI
from datetime import datetime

app = FastAPI()


@app.get("/")
def read_root():
    return {"message": "Hello FastAPI!", "timestamp": str(datetime.now())}
