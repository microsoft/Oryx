// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class BuildScriptGeneratorOptions
    {
        public string SourceDir { get; set; }

        public string IntermediateDir { get; set; }

        public bool Inline { get; set; }

        public string DestinationDir { get; set; }

        public string Language { get; set; }

        public string LanguageVersion { get; set; }

        public string TempDir { get; set; }

        public string LogFile { get; set; }

        public LogLevel MinimumLogLevel { get; set; }

        public bool ScriptOnly { get; set; }

        public bool Force { get; set; }
    }
}