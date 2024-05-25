var express = require('express');
var app = express();
app.use(express.static(__dirname + '/public'));

app.get('/', function (req, res) {
    res.sendFile(__dirname + '/public/index.html');
});

app.get('/api', function (req, res) {
    res.send('Hello from webfrontend');
});

var port = process.env.PORT || 8080;
var server = app.listen(port, function () {
    console.log('Listening on port ' + port);
});

process.on("SIGINT", () => {
    process.exit(130 /* 128 + SIGINT */);
});

process.on("SIGTERM", () => {
    console.log("Terminating...");
    server.close();
});
