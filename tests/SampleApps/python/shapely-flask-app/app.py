import os
import math
from flask import Flask
from flask import Response
from flask import json
from shapely.geometry import Point

app = Flask(__name__)

@app.route('/')
def index():
    patch = Point(0.0, 0.0).buffer(10.0)
    patch
    print ("Area is: " + str(patch.area))
    print("Index method is called.")
    return "Hello Shapely, Area is: "+ str(math.ceil(patch.area))

@app.route('/cities.json')
def cities():
    data = {"cities" : ["Amsterdam","Berlin","New York","San Francisco","Tokyo"]}
    resp = Response(json.dumps(data), status=200, mimetype='application/json')
    return resp

if __name__ == '__main__':
    port = int(os.environ.get('PORT', 3000))
    app.run(host='0.0.0.0', port=port, debug=True)