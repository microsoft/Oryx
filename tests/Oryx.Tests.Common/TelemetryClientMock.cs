// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;

public class TelemetryClientMock
{
    public virtual string connectionString { get; set; }
    private TelemetryClient telemetryClient;
    private TelemetryConfiguration telemetryConfigutration;

    public TelemetryClientMock()
    {
        this.telemetryConfigutration = new TelemetryConfiguration();
        this.telemetryClient = new TelemetryClient(this.telemetryConfigutration);
    }
    public TelemetryClient GetTelemetryClient()
    {
        this.telemetryConfigutration = new TelemetryConfiguration()
        {
            ConnectionString = this.connectionString,
        };
        return new TelemetryClient(telemetryConfigutration);
    }
}