from flask import Flask, jsonify
import psycopg2

app = Flask(__name__)

class Database:
    def listProductNames(self):
        conn = psycopg2.connect("dbname='oryxdb' user='oryxuser' host='dbserver' password='Passw0rd'")
        try:
            c = conn.cursor()
            c.execute("SELECT Name FROM Products")
            result = c.fetchall()
        finally:
            conn.close()
        return result

@app.route('/')
def listProducts():
    db = Database()
    rows = db.listProductNames()
    payload = []
    content = {}
    for row in rows:
        content = {'Name': row[0]}
        payload.append(content)
        content = {}
    return jsonify(payload)