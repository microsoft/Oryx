// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.NLogTarget;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Defines extension methods for direct interaction with Application Insights.
    /// </summary>
    public static class LoggerAiExtensions
    {
        private const int AiMessageLengthLimit = 32768;

        /// <summary>
        /// Logs dependency specifications for a processed repository.
        /// </summary>
        public static void LogDependencies(
            this ILogger logger,
            string platform,
            string platformVersion,
            IEnumerable<string> depSpecs,
            bool devDeps = false)
        {
            var client = GetTelemetryClient();
            var props = new Dictionary<string, string>
            {
                { nameof(platform),        platform },
                { nameof(platformVersion), platformVersion },
            };

            string devPrefix = devDeps ? "Dev " : string.Empty;
            foreach (string dep in depSpecs)
            {
                client.TrackTrace(
                    $"{devPrefix}Dependency: {dep.ReplaceUrlUserInfo()}",
                    ApplicationInsights.DataContracts.SeverityLevel.Information,
                    props);
            }
        }

        public static void LogEvent(this ILogger logger, string eventName, IDictionary<string, string> props = null)
        {
            GetTelemetryClient().TrackEvent(eventName, props);
        }

        public static void LogTrace(this ILogger logger, string message, IDictionary<string, string> props = null)
        {
            GetTelemetryClient().TrackTrace(message, props);
        }

        /// <summary>
        /// Logs a long message in chunks, with each chunk limited in length to 2^15.
        /// </summary>
        /// <param name="logger">An instance of an <see cref="ILogger"/>.</param>
        /// <param name="level">The level of logging; will apply to all chunks.</param>
        /// <param name="header">The chunk header; will be followed by chunk index, a colon, and a line break.</param>
        /// <param name="nonFormattedMessage">The long message to be chunkified and logged.</param>
        public static void LogLongMessage(
            this ILogger logger,
            LogLevel level,
            [NotNull] string header,
            string nonFormattedMessage,
            IDictionary<string, object> properties)
        {
            int maxChunkLen = AiMessageLengthLimit - header.Length - 16; // 16 should cover for the header formatting
            int i = 0;
            var chunks = nonFormattedMessage.Chunkify(maxChunkLen);
            foreach (string chunk in chunks)
            {
                logger.Log(level, $"{header} ({++i}/{chunks.Count}):\n{chunk}", properties: properties);
            }
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
            // Temporarily use obsolete empty client as mentioned in work item 1735437
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
