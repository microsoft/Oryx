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
        private readonly ILogger logger;

        private StreamWriter fileStream;

        public FileOutputHandler(StreamWriter filestream, ILogger logger)
        {
            this.fileStream = filestream;
            this.logger = logger;
        }

        public void Handle(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                this.fileStream.Write(outLine.Data + "\n");
                this.logger.LogInformation(outLine.Data);
            }
        }
    }
}
