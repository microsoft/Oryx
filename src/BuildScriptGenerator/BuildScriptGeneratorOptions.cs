// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class BuildScriptGeneratorOptions
    {
        public string SourceDir { get; set; }

        public string IntermediateDir { get; set; }

        public string DestinationDir { get; set; }

        public string Language { get; set; }

        public string LanguageVersion { get; set; }

        public string LogFile { get; set; }

        public LogLevel MinimumLogLevel { get; set; }

        public bool ScriptOnly { get; set; }

        public IDictionary<string, string> Properties { get; set; }
    }
}