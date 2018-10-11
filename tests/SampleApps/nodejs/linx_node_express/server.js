var express = require('express');
var app = express();

app.get('/', function (req, res) {
  res.send('Hello World from express!');
});

var port = process.env.PORT || 80;
app.listen(port, function () {
  console.log('Example app listening on port ' + port);
});
