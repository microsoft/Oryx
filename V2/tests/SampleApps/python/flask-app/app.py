from flask import Flask
from datetime import datetime
from flask import current_app

app = Flask(__name__)

@app.route("/")
def hello():
    return "Hello World! " + str(datetime.now())

@app.route("/applicationpath")
def applicationPath():
    return current_app.root_path

