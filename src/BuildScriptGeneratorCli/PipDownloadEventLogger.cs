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

        private readonly ILogger logger;

        private EventStopwatch currentDownload;

        public PipDownloadEventLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void ProcessLine(string line)
        {
            var cleanLine = line.Trim().ReplaceUrlUserInfo();

            if (this.currentDownload != null)
            {
                this.currentDownload.AddProperty("nextLine", cleanLine);
                this.currentDownload.Dispose();
                this.currentDownload = null;
            }

            if (line.Contains(PipDownloadMarker))
            {
                var parts = line.Substring(line.IndexOf(PipDownloadMarker)).Split(" ");

                if (parts[1].StartsWith("http"))
                {
                    this.currentDownload = this.logger.LogTimedEvent(EventName);
                    this.currentDownload.AddProperty("markerLine", cleanLine);

                    var url = parts[1].ReplaceUrlUserInfo();
                    this.currentDownload.AddProperty(nameof(url), url);

                    var size = parts.Last();
                    if (size.StartsWith('(') && size.EndsWith(')'))
                    {
                        this.currentDownload.AddProperty(nameof(size), size.Trim('(', ')'));
                    }
                }
            }
        }
    }
}
