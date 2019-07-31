from flask import Flask
from datetime import datetime

app = Flask(__name__)

@app.route("/")
def hello():
    return "Hello World! " + str(datetime.now())

