import os
from flask import Flask
from flask import Response
from flask import json

app = Flask(__name__)

@app.route('/')
def index():
    print 'Index method is called.'
    return "Hello World!"

@app.route('/cities.json')
def cities():
    data = {"cities" : ["Amsterdam","Berlin","New York","San Francisco","Tokyo"]}
    resp = Response(json.dumps(data), status=200, mimetype='application/json')
    return resp

if __name__ == '__main__':
    port = int(os.environ.get('PORT', 5000))
    app.run(host='0.0.0.0', port=port, debug=True)
