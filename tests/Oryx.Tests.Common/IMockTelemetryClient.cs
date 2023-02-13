using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;

public interface ITelemetyClientMock
{}

public class TelemetryClientMock : ITelemetyClientMock
{
    public string connectionString { get; set; }
    private TelemetryClient telemetryClient;
    private TelemetryConfiguration telemetryConfigutration = new TelemetryConfiguration();
    public TelemetryClientMock()
    {
        telemetryClient = new TelemetryClient(telemetryConfigutration);
    }
}
