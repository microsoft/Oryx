// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ApplicationInsights;

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    public class EventStopwatch : IDisposable
    {
        private readonly TelemetryClient client;
        private readonly string eventName;
        private readonly Stopwatch stopwatch;
        private IDictionary<string, string> eventProps;

        public EventStopwatch(
            TelemetryClient telemetryClient,
            string eventName,
            IDictionary<string, string> eventProperties)
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

        public void SetProperties(Dictionary<string, string> props)
        {
            this.eventProps = props;
        }

        public void Dispose()
        {
            this.stopwatch.Stop();
            this.client?.TrackEvent(
                this.eventName,
                this.eventProps,
                new Dictionary<string, double> { { "processingTime", this.stopwatch.Elapsed.TotalMilliseconds } });
        }
    }
}
