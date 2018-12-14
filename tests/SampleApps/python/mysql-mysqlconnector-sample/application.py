from flask import Flask, jsonify
import mysql.connector

app = Flask(__name__)

class Database:
    def listProductNames(self):
        conn = mysql.connector.connect(user='oryxuser', password='Passw0rd', host='dbserver', database='oryxdb')
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