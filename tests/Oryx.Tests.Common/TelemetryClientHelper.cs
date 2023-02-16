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
    public static string aiKey = TestConstants.AiKey;
    public static TelemetryClient GetTelemetryClient()
    {
        var telemetryConfig = new TelemetryConfiguration();
        if (!string.IsNullOrWhiteSpace(aiKey))
        {
            telemetryConfig.ConnectionString = aiKey;
        }
        return new TelemetryClient(telemetryConfig);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
    }
}