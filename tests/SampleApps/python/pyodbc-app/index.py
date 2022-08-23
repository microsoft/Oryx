from flask import Flask, jsonify
import pyodbc
import sys, os

app = Flask(__name__)

cnxn = None
cursor = None

messages = []

def Connect():
    messages.clear()
    global cnxn
    cnxn = pyodbc.connect('DRIVER={ODBC Driver 17 for SQL Server};SERVER=;DATABASE=;UID=;PWD=')
    global cursor 
    cursor = cnxn.cursor()
    messages.append("Connecting to Database")

def CreateTable():
    #Check if table exists
    if cursor.tables(table='Users', tableType='TABLE').fetchone():
        DeleteTable()
    
    cursor.execute("""
    CREATE TABLE Users
    (id int IDENTITY(1,1) PRIMARY KEY,
    name varchar(50) NOT NULL,
    lastname varchar(50) NULL)
    """)
    cnxn.commit()
    messages.append("Creating Table Users")

def QueryRow():
    cursor.execute("SELECT * FROM Users where id=1") 
    row = cursor.fetchone() 
    messages.append("Querying from Table and selecting 1")
    while row: 
        print(row[0])
        row = cursor.fetchone()

def QueryAllRows():
    cursor.execute("SELECT * FROM Users") 
    rows = cursor.fetchall()
    messages.append("Querying all rows from Table")
    for row in rows:
        print(row, end='\n')
    
def InsertRow():
    cursor.execute("""
    INSERT INTO Users (name, lastname)
    VALUES (?, ?)""", ('Name','LastName')) 
    cnxn.commit()
    messages.append("Inserting row")

def DeleteRow():
    cursor.execute("""
    DELETE FROM Users where id=1""")  
    cnxn.commit()
    messages.append("Deleting row")

def DeleteTable():
    cursor.execute("""
    DROP TABLE Users
    """)
    cnxn.commit()
    messages.append("Dropping Table Users")

@app.route('/')
def home():
    try:
        Connect()
        CreateTable()
        InsertRow()
        QueryRow()
        QueryAllRows()
        DeleteRow()
        DeleteTable()
    except Exception as ex:
        print("Raised exception caught: ", ex.args)
    finally:
        cursor.close()
        cnxn.close()
        messages.append("Closing connection to Database")
        
    return jsonify(messages)

if __name__ == '__main__':
    app.run()