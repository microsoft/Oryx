// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator.Common.Extensions
{
    /// <summary>
    /// Defines extension methods for direct interaction with Application Insights via TelemetryClient.
    /// </summary>
    public static class TelemetryClientExtensions
    {
        /// <summary>
        /// Logs dependency specifications for a processed repository.
        /// </summary>
        public static void LogDependencies(
               this TelemetryClient telemetryClient,
               string platform,
               string platformVersion,
               IEnumerable<string> depSpecs,
               bool devDeps = false)
        {
            var props = new Dictionary<string, string>
            {
                { nameof(platform),        platform },
                { nameof(platformVersion), platformVersion },
            };

            string devPrefix = devDeps ? "Dev " : string.Empty;
            foreach (string dep in depSpecs)
            {
                telemetryClient.TrackTrace(
                    $"{devPrefix}Dependency: {dep.ReplaceUrlUserInfo()}",
                    ApplicationInsights.DataContracts.SeverityLevel.Information,
                    props);
            }
        }

        public static void LogEvent(this TelemetryClient telemetryClient, string eventName, IDictionary<string, string> props = null)
        {
            telemetryClient.TrackEvent(eventName, props);
        }

        public static void LogTimedEvent(this TelemetryClient telemetryClient, string eventName, double processingTime, IDictionary<string, string> props = null)
        {
            telemetryClient.TrackEvent(
                eventName,
                props,
                new Dictionary<string, double> { { "processingTime", processingTime } });
        }

        public static void LogTrace(this TelemetryClient telemetryClient, string message, IDictionary<string, string> props = null)
        {
            telemetryClient.TrackTrace(message, props);
        }

        public static string StartOperation(this TelemetryClient telemetryClient, string name)
        {
            var op = telemetryClient.StartOperation<ApplicationInsights.DataContracts.RequestTelemetry>(name);
            return op.Telemetry.Id;
        }

        public static EventStopwatch LogTimedEvent(this TelemetryClient telemetryClient, string eventName, IDictionary<string, string> props = null)
        {
            return new EventStopwatch(telemetryClient, eventName, props);
        }
    }
}