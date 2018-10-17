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
                string key = null;
                string value = null;

                // We only care about the first instance of '=' even if there are multiple
                // (for example, the value itself could have that symbol in it)
                var equalToSymbolIndex = property.IndexOf('=');
                if (equalToSymbolIndex < 0)
                {
                    // Example: -p showlog (in this case the user might not want to give a value)
                    key = property;
                    value = string.Empty;
                }
                else if (equalToSymbolIndex == 0)
                {
                    throw new InvalidUsageException($"Property key cannot start with '=' for property '{property}'.");
                }
                else
                {
                    // -p showlog=true
                    key = property.Substring(0, equalToSymbolIndex);
                    value = property.Substring(equalToSymbolIndex + 1);
                }

                key = key.Trim('"');
                value = value.Trim('"');

                propertyList[key] = value;
            }
            return propertyList;
        }
    }
}
