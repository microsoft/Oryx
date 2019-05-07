// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.NLogTarget;
using Microsoft.Oryx.Common;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Defines extension methods for direct interaction with Application Insights.
    /// </summary>
    public static class LoggerAiExtensions
    {
        private const int AiMessageLengthLimit = 2 ^ 15;

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

        public static void LogTrace(this ILogger logger, string message, IDictionary<string, string> props = null)
        {
            GetTelemetryClient().TrackTrace(message, props);
        }

        /// <summary>
        /// Logs a long message in chunks, with each chunk limited in length to 2^15.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="level"></param>
        /// <param name="header"></param>
        /// <param name="message"></param>
        public static void LogLongMessage(this ILogger logger, LogLevel level, [NotNull] string header, string message)
        {
            int maxChunkLen = AiMessageLengthLimit - header.Length - 16; // 16 should cover for the header formatting
            int i = 0;
            var chunks = Chunkify(message, maxChunkLen);
            foreach (string chunk in chunks)
            {
                logger.Log(level, $"{header} ({++i}/{chunks.Count}):\n{chunk}");
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
            var client = new TelemetryClient();

            ApplicationInsightsTarget aiTarget = (ApplicationInsightsTarget)NLog.LogManager.Configuration?.FindTargetByName("ai");
            if (aiTarget != null)
            {
                client.Context.InstrumentationKey = aiTarget.InstrumentationKey;
            }

            return client;
        }

        public static IList<string> Chunkify(string str, int maxLength)
        {
            var result = new List<string>();
            for (int i = 0; i < str.Length; i += maxLength)
            {
                result.Add(str.Substring(i, Math.Min(maxLength, str.Length - i)));
            }
            return result;
        }
    }
}
