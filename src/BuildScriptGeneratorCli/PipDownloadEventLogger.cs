// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class PipDownloadEventLogger : ITextStreamProcessor
    {
        private const string PipDownloadMarker = "Downloading";
        private const string EventName = "PipDownload";

        private readonly ILogger _logger;

        private EventStopwatch _currentDownload = null;

        public PipDownloadEventLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void ProcessLine(string line)
        {
            if (_currentDownload != null)
            {
                _currentDownload.Dispose();
                _currentDownload = null;
            }

            if (line.Contains(PipDownloadMarker))
            {
                var parts = line.Substring(line.IndexOf(PipDownloadMarker)).Split(" ");

                if (parts[1].StartsWith("http"))
                {
                    _currentDownload = _logger.LogTimedEvent(EventName);

                    var url = parts[1];
                    _currentDownload.AddProperty(nameof(url), url);

                    var size = parts.Last();
                    if (size.StartsWith('(') && size.EndsWith(')'))
                    {
                        _currentDownload.AddProperty(nameof(size), size.Trim('(', ')'));
                    }
                }
            }
        }
    }
}
