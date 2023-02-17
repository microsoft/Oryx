// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Oryx.Tests.Common;
using System;

public static class TelemetryClientHelper
{
    public static TelemetryClient GetTelemetryClient()
    {
        var connectionString = Environment.GetEnvironmentVariable(TestConstants.AppInsightsConnectionStringEnvironmentVariable)
         ?? TestConstants.AppInsightsConnectionString;
        var telemetryConfig = new TelemetryConfiguration()
        {
            ConnectionString = connectionString,
        };
        return new TelemetryClient(telemetryConfig);
    }
}