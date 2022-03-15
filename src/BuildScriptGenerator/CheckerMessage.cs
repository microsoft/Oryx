// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class CheckerMessage : ICheckerMessage
    {
        public CheckerMessage(string content, LogLevel level = LogLevel.Warning)
        {
            this.Level = level;
            this.Content = content;
        }

        public LogLevel Level { get; }

        public string Content { get; }
    }
}
