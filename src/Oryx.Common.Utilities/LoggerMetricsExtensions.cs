// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.NLogTarget;

namespace Microsoft.Extensions.Logging
{
    public static class LoggerMetricsExtensions
    {
        public static void LogEvent(this ILogger logger, string eventName, IDictionary<string, string> props = null)
        {
            GetTelemetryClient()?.TrackEvent(eventName, props);
        }

        public static EventStopwatch LogTimedEvent(this ILogger logger, string eventName, IDictionary<string, string> props = null)
        {
            return new EventStopwatch(GetTelemetryClient(), eventName, props);
        }

        private static TelemetryClient GetTelemetryClient()
        {
            ApplicationInsightsTarget aiTarget = (ApplicationInsightsTarget)NLog.LogManager.Configuration?.FindTargetByName("ai");
            if (aiTarget == null)
            {
                return null;
            }

            var client = new TelemetryClient();
            client.Context.InstrumentationKey = aiTarget.InstrumentationKey;
            return client;
        }
    }

    public class EventStopwatch : IDisposable
    {
        private readonly TelemetryClient client;
        private readonly string eventName;
        private readonly IDictionary<string, string> eventProps;
        private readonly Stopwatch stopwatch;

        public EventStopwatch(TelemetryClient telemetryClient, string eventName, IDictionary<string, string> eventProperties)
        {
            this.client = telemetryClient;
            this.eventName = eventName;
            this.eventProps = eventProperties ?? new Dictionary<string, string>();
            this.stopwatch = Stopwatch.StartNew();
        }

        public void AddProperty(string name, string value)
        {
            this.eventProps.Add(name, value);
        }

        public void Dispose()
        {
            this.stopwatch.Stop();
            this.client?.TrackEvent(this.eventName, this.eventProps, new Dictionary<string, double> { { "processingTime", stopwatch.Elapsed.TotalMilliseconds } });
        }
    }
}
