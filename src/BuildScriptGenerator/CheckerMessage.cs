// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class CheckerMessage : ICheckerMessage
    {
        private readonly LogLevel _level;

        private readonly string _content;

        public CheckerMessage(string content, LogLevel level = LogLevel.Warning)
        {
            _level = level;
            _content = content;
        }

        public LogLevel Level => _level;

        public string Content => _content;
    }
}
