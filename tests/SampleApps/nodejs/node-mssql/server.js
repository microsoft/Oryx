var express = require("express");
var app = express();
var connection = require('tedious').Connection;
var request = require('tedious').Request;

app.get('/', function (req, res) {
    // Create connection to database
    var config = {
        userName: process.env.SQLSERVER_DATABASE_USERNAME,
        password: process.env.SQLSERVER_DATABASE_PASSWORD,
        server: process.env.SQLSERVER_DATABASE_HOST,
        options: {
            database: process.env.SQLSERVER_DATABASE_NAME,
            encrypt: true
        }
    }
    var conn = new connection(config);

    conn.on('connect', function(err) {
        if (err) {
            console.log('Error: ' + err);
        } else {
            sqlreq = new request("SELECT * FROM Products FOR JSON AUTO", function(err, rowCount) {
                if (err) {
                    console.log('Error: ' + err);
                }

                console.log('Row count:' + rowCount);
            });

            sqlreq.on('row', function(columns) { 
                columns.forEach(function(column) {  
                    if (column.value === null) {  
                        console.log('NULL');
                    } else {  
                        res.send(column.value);
                    }  
                });
            });

            conn.execSql(sqlreq); 
        }
    });
})  

var server = app.listen(process.env.PORT, function () {
    console.log("Listening on port %s...", server.address().port);
});