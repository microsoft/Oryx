var express = require('express');
var app = express();
var appInsights = require('applicationinsights');

var responseString = "AppInsights is not configured!";
var setupString = process.env.APPLICATIONINSIGHTS_CONNECTION_STRING || process.env.APPINSIGHTS_INSTRUMENTATIONKEY;

console.log(setupString);
console.log(process.env.ApplicationInsightsAgent_EXTENSION_VERSION)

if (setupString && process.env.ApplicationInsightsAgent_EXTENSION_VERSION
    && process.env.ApplicationInsightsAgent_EXTENSION_VERSION !== "disabled") {
	console.log("hello world here")
	appInsights
		.setup(setupString)
		.setSendLiveMetrics(true)
		.start();
    responseString = "AppInsights is set to send telemetry!"

    let client = appInsights.defaultClient;
    client.trackTrace({message: "trace message"});
    client.trackMetric({name: "custom metric", value: 3});
    
}


app.get('/', function (req, res) {
  res.send(responseString);
});

var port = process.env.PORT || 80;
app.listen(port, function () {
  console.log('Example app listening on port ' + port);
});
