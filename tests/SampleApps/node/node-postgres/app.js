const express = require('express')
const app = express()
var pg = require('pg')
var PGUSER = 'oryxuser'
var PGDATABASE = 'oryxdb'
var PASSWORD = process.env.DATABASE_PASSWORD
var HOST = 'dbserver'

var config = {
  host: HOST,
  user: PGUSER,
  database: PGDATABASE,
  password: PASSWORD
}

var pool = new pg.Pool(config)
var myClient

pool.connect(function (err, client, done) {
  if (err) console.log(err)

  app.listen(8000, function () {
    console.log('listening on 8000')
  })
  
  myClient = client
  app.get('/', (req, res) => {
    let query = "SELECT Name FROM Products";
    myClient.query(query, (err, result) => {
        if (err) {
            console.log(err);
            res.redirect('/');
        }
        var names = [];
        result.rows.forEach(function(row) {
          var product = {};
          product.Name = row.name;
          names.push(product);
        });
        res.send(JSON.stringify(names));
    });
  })
})
