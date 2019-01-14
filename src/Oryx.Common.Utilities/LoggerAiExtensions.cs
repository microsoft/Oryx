// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.NLogTarget;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Defines extension methods for direct interaction with Application Insights.
    /// </summary>
    public static class LoggerAiExtensions
    {
        /// <summary>
        /// Logs dependency specifications for a processed repository.
        /// </summary>
        public static void LogDependencies(this ILogger logger, string platform, string platformVersion, IEnumerable<string> dependencySpecs)
        {
            var client = GetTelemetryClient();
            var props = new Dictionary<string, string>
            {
                { nameof(platform),        platform },
                { nameof(platformVersion), platformVersion }
            };

            foreach (string dep in dependencySpecs)
            {
                client.TrackTrace($"Dependency: {dep}", ApplicationInsights.DataContracts.SeverityLevel.Information, props);
            }
        }

        public static void LogEvent(this ILogger logger, string eventName, IDictionary<string, string> props = null)
        {
            GetTelemetryClient().TrackEvent(eventName, props);
        }

        public static string StartOperation(this ILogger logger, string name)
        {
            var op = GetTelemetryClient().StartOperation<ApplicationInsights.DataContracts.RequestTelemetry>(name);
            return op.Telemetry.Id;
        }

        public static EventStopwatch LogTimedEvent(this ILogger logger, string eventName, IDictionary<string, string> props = null)
        {
            return new EventStopwatch(GetTelemetryClient(), eventName, props);
        }

        private static TelemetryClient GetTelemetryClient()
        {
            var client = new TelemetryClient();

            ApplicationInsightsTarget aiTarget = (ApplicationInsightsTarget)NLog.LogManager.Configuration?.FindTargetByName("ai");
            if (aiTarget != null)
            {
                client.Context.InstrumentationKey = aiTarget.InstrumentationKey;
            }

            return client;
        }
    }
}
