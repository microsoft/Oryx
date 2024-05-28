const express = require('express');
const mysql = require('mysql');
const app = express();

const db = mysql.createConnection ({
    host: 'dbserver',
    user: 'oryxuser',
    password: process.env.DATABASE_PASSWORD,
    database: 'oryxdb'
});

// connect to database
db.connect((err) => {
    if (err) {
        throw err;
    }
    console.log('Connected to database');
});
global.db = db;

app.get('/', (req, res) => {
    let query = "SELECT Name FROM `Products`";
    
    // execute query
    db.query(query, (err, result) => {
        if (err) {
            console.log(err);
            res.redirect('/');
        }
        res.send(result);
    });
});

// set the app to listen on the port
const port = 8000;
app.set('port', process.env.port || port); // set express to use this port
app.listen(port, () => {
    console.log(`Server running on port: ${port}`);
});