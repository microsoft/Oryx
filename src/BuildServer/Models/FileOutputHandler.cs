// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildServer.Models
{
    public class FileOutputHandler
    {
        private readonly ILogger _logger;

        private StreamWriter _fileStream;

        public FileOutputHandler(StreamWriter filestream, ILogger logger)
        {
            _fileStream = filestream;
            _logger = logger;
        }

        public void Handle(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                _fileStream.Write(outLine.Data + "\n");
                _logger.LogInformation(outLine.Data);
            }
        }
    }
}
