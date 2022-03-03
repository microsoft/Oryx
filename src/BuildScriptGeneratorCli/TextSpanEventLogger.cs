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
        private readonly ILogger _logger;

        private readonly Dictionary<string, TextSpan> _beginnings = new Dictionary<string, TextSpan>();

        private readonly Dictionary<string, TextSpan> _endings = new Dictionary<string, TextSpan>();

        private readonly Dictionary<TextSpan, EventStopwatch> _events = new Dictionary<TextSpan, EventStopwatch>();

        public TextSpanEventLogger(ILogger logger, TextSpan[] events)
        {
            _logger = logger;

            foreach (var span in events)
            {
                _beginnings[span.BeginMarker] = span;
                _endings[span.EndMarker] = span;
            }
        }

        public void ProcessLine(string line)
        {
            var marker = line.Trim();

            // Start measuring
            if (_beginnings.ContainsKey(marker))
            {
                var span = _beginnings[marker];

                // Avoid a new measurement for a span already being measured
                if (!_events.ContainsKey(span))
                {
                    _events[span] = _logger.LogTimedEvent(span.Name);
                }
            }
            else if (_endings.ContainsKey(marker))
            {
                // Stop a running measurement
                var span = _endings[marker];
                _events.GetValueOrDefault(span)?.Dispose(); // Records the measurement
                _events.Remove(span); // No need to check if the removal succeeded, because the event might not exist
            }
        }
    }
}
