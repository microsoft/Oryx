// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

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
            bool scriptOnly,
            string[] properties)
        {
            options.SourceDir = string.IsNullOrEmpty(sourceDir) ? Directory.GetCurrentDirectory() : Path.GetFullPath(sourceDir);
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

            // Process properties
            if (properties != null)
            {
                options.Properties = ProcessProperties(properties);
            }
        }

        // To enable testing
        internal static IDictionary<string, string> ProcessProperties(string[] properties)
        {
            var propertyList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in properties)
            {
                if (NameAndValuePairParser.TryParse(property, out var key, out var value))
                {
                    key = key.Trim('"');
                    value = value.Trim('"');

                    propertyList[key] = value;
                }
                else
                {
                    throw new InvalidUsageException($"Property key cannot start with '=' for property '{property}'.");
                }
            }
            return propertyList;
        }
    }
}
