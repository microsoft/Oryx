from flask import Flask, jsonify
import MySQLdb

app = Flask(__name__)

class Database:
    def listProductNames(self):
        host = "dbserver"
        user = "oryxuser"
        password = "Passw0rd"
        databaseName = "oryxdb"

        db=MySQLdb.connect(host=host, user=user, password=password, db=databaseName)
        c=db.cursor()
        c.execute("SELECT Name FROM Products")
        result = c.fetchall()
        return result

@app.route('/')
def products():

    def db_query():
        db = Database()
        productNames = db.listProductNames()
        return productNames

    names = db_query()
    return jsonify(names)

