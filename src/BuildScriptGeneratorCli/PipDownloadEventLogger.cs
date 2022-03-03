// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Detects and measures time for `pip` package downloads in a stream of text.
    /// </summary>
    internal class PipDownloadEventLogger : ITextStreamProcessor
    {
        private const string PipDownloadMarker = "Downloading";
        private const string EventName = "PipDownload";

        private readonly ILogger _logger;

        private EventStopwatch _currentDownload;

        public PipDownloadEventLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void ProcessLine(string line)
        {
            var cleanLine = line.Trim().ReplaceUrlUserInfo();

            if (_currentDownload != null)
            {
                _currentDownload.AddProperty("nextLine", cleanLine);
                _currentDownload.Dispose();
                _currentDownload = null;
            }

            if (line.Contains(PipDownloadMarker))
            {
                var parts = line.Substring(line.IndexOf(PipDownloadMarker)).Split(" ");

                if (parts[1].StartsWith("http"))
                {
                    _currentDownload = _logger.LogTimedEvent(EventName);
                    _currentDownload.AddProperty("markerLine", cleanLine);

                    var url = parts[1].ReplaceUrlUserInfo();
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
