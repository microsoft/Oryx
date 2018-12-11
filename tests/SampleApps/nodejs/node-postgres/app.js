const express = require('express')
const app = express()
var pg = require('pg')
var format = require('pg-format')
var PGUSER = 'oryxuser'
var PGDATABASE = 'oryxdb'
var PASSWORD = 'Passw0rd'
var HOST = 'dbserver'
var age = 732

var config = {
  host: HOST,
  user: PGUSER, // name of the user account
  database: PGDATABASE, // name of the database
  password: PASSWORD,
  max: 10, // max number of clients in the pool
  idleTimeoutMillis: 30000 // how long a client is allowed to remain idle before being closed
}

var pool = new pg.Pool(config)
var myClient

pool.connect(function (err, client, done) {
  if (err) console.log(err)

  app.listen(5000, function () {
    console.log('listening on 5000')
  })
  
  myClient = client
  app.get('/', (req, res) => {
    var ageQuery = format('SELECT * from numbers WHERE age = %L', age)
    myClient.query(ageQuery, function (err, result) {
      if (err) {
        res.send(err)
      }
      res.send(result.rows[0])
    })
  })
})
