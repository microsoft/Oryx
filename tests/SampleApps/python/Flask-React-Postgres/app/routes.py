from flask import Flask, jsonify
from datetime import datetime

app = Flask(__name__)


@app.route("/api/hello", methods=["GET"])
def hello():
    return jsonify({
        'message': 'Hello from Flask! This is a simple API response.',
        'status': 'success',
        'timestamp': datetime.now().isoformat()
    })


@app.route('/')
def index():
    return '<h1>Flask Backend Running!</h1><p>API endpoint: <a href="/api/hello">/api/hello</a></p>'


if __name__ == '__main__':
    app.run(debug=True, port=5000)
