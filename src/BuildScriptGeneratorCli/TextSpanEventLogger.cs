// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Detects and measures time for events within a textual stream, defined by a beginning and an ending marker.
    /// </summary>
    internal class TextSpanEventLogger : ITextStreamProcessor
    {
        private readonly ILogger logger;

        private readonly Dictionary<string, TextSpan> beginnings = new Dictionary<string, TextSpan>();

        private readonly Dictionary<string, TextSpan> endings = new Dictionary<string, TextSpan>();

        private readonly Dictionary<TextSpan, EventStopwatch> events = new Dictionary<TextSpan, EventStopwatch>();

        public TextSpanEventLogger(ILogger logger, TextSpan[] events)
        {
            this.logger = logger;

            foreach (var span in events)
            {
                this.beginnings[span.BeginMarker] = span;
                this.endings[span.EndMarker] = span;
            }
        }

        public void ProcessLine(string line)
        {
            var marker = line.Trim();

            // Start measuring
            if (this.beginnings.ContainsKey(marker))
            {
                var span = this.beginnings[marker];

                // Avoid a new measurement for a span already being measured
                if (!this.events.ContainsKey(span))
                {
                    this.events[span] = this.logger.LogTimedEvent(span.Name);
                }
            }
            else if (this.endings.ContainsKey(marker))
            {
                // Stop a running measurement
                var span = this.endings[marker];
                this.events.GetValueOrDefault(span)?.Dispose(); // Records the measurement
                _ = this.events.Remove(span); // No need to check if the removal succeeded, because the event might not exist
            }
        }
    }
}
