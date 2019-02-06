from flask import Flask, jsonify
import pyodbc
import os

app = Flask(__name__)

class Database:
    def listProductNames(self):
        host = "dbserver"
        databaseName = "oryxdb"
        user = "sa"
        password = os.getenv('DATABASE_PASSWORD')

        conn = pyodbc.connect('DRIVER={ODBC Driver 17 for SQL Server};SERVER='+host+';DATABASE='+databaseName+';UID='+user+';PWD='+ password)
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