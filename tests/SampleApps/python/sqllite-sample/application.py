from flask import Flask, jsonify
import sqlite3

app = Flask(__name__)

class Database:
    def listProductNames(self):
        conn = sqlite3.connect("oryxdb.db")
        try:
            c = conn.cursor()
            c.execute("CREATE TABLE Products(Name varchar(50))")
            c.execute("INSERT INTO Products VALUES('Car')")
            c.execute("INSERT INTO Products VALUES('Television')")
            c.execute("INSERT INTO Products VALUES('Table')")
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