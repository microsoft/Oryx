using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator.Common.Extensions
{
    public static class TelemetryClientExtension
    {
        public static void LogDependencies(
               this IOptions<TelemetryClient> telemetryClient,
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
                telemetryClient.Value.TrackTrace(
                    $"{devPrefix}Dependency: {dep.ReplaceUrlUserInfo()}",
                    ApplicationInsights.DataContracts.SeverityLevel.Information,
                    props);
            }
        }

        public static void LogEvent(this IOptions<TelemetryClient> telemetryClient, string eventName, IDictionary<string, string> props = null)
        {
            telemetryClient.Value.TrackEvent(eventName, props);
        }

        public static void LogTrace(this IOptions<TelemetryClient> telemetryClient, string message, IDictionary<string, string> props = null)
        {
            telemetryClient.Value.TrackTrace(message, props);
        }

        public static string StartOperation(this IOptions<TelemetryClient> telemetryClient, string name)
        {
            var op = telemetryClient.Value.StartOperation<ApplicationInsights.DataContracts.RequestTelemetry>(name);
            return op.Telemetry.Id;
        }

        public static EventStopwatch LogTimedEvent(this IOptions<TelemetryClient> telemetryClient, string eventName, IDictionary<string, string> props = null)
        {
            return new EventStopwatch(telemetryClient.Value, eventName, props);
        }
    }
}