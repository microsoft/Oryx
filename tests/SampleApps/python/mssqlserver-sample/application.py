from flask import Flask, jsonify
import pyodbc
import os

app = Flask(__name__)

@app.errorhandler(Exception)
def exception_handler(exc):
    return repr(exc)

class Database:
    def listProductNames(self):
        host = os.getenv('DATABASE_HOSTNAME')
        databaseName = os.getenv('DATABASE_NAME')
        user = os.getenv('DATABASE_USERNAME')
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
    for row in rows:
        payload.append({'Name': row[0]})
    return jsonify(payload)