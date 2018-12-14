from flask import Flask, jsonify
import MySQLdb

app = Flask(__name__)

class Database:
    def listProductNames(self):
        conn=MySQLdb.connect(host='dbserver', user='oryxuser', password='Passw0rd', db='oryxdb')
        try:
            c = conn.cursor()
            c.execute("SELECT Name FROM Products")
            result = c.fetchall()
        finally:
            conn.close()
        return result

@app.route('/')
def products():
    db = Database()
    rows = db.listProductNames()
    payload = []
    content = {}
    for row in rows:
        content = {'Name': row[0]}
        payload.append(content)
        content = {}
    return jsonify(payload)

