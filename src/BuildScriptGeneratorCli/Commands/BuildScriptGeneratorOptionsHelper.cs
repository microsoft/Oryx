// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class BuildScriptGeneratorOptionsHelper
    {
        public static void ConfigureBuildScriptGeneratorOptions(
            BuildScriptGeneratorOptions options,
            string sourceDir,
            string destinationDir,
            string intermediateDir,
            string language,
            string languageVersion,
            string logFile,
            bool scriptOnly)
        {
            options.SourceDir = Path.GetFullPath(sourceDir);
            options.Language = language;
            options.LanguageVersion = languageVersion;

            if (!string.IsNullOrEmpty(destinationDir))
            {
                options.DestinationDir = Path.GetFullPath(destinationDir);
            }

            if (!string.IsNullOrEmpty(intermediateDir))
            {
                options.IntermediateDir = Path.GetFullPath(intermediateDir);
            }

            if (!string.IsNullOrEmpty(logFile))
            {
                options.LogFile = Path.GetFullPath(logFile);
            }

            options.MinimumLogLevel = LogLevel.Trace;
            options.ScriptOnly = scriptOnly;
        }
    }
}
