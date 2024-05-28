var express = require('express');
var app = express();

var responseString = "AppInsights is not configured!";
var setupString = process.env.APPLICATIONINSIGHTS_CONNECTION_STRING || process.env.APPINSIGHTS_INSTRUMENTATIONKEY;

console.log(setupString);
console.log(process.env.ApplicationInsightsAgent_EXTENSION_VERSION)

app.get('/', function (req, res) {
// Check for incoming request flag set by Node.js SDK to determine if the SDK is instrumented or not
//for version 1.7.2 onward
  if (req["_appInsightsAutoCollected"] === true) {
    responseString = "AppInsights is set to send telemetry!";
  }
  res.send(responseString);
});

var port = process.env.PORT || 80;
app.listen(port, function () {
  console.log('Example app listening on port ' + port);
});
