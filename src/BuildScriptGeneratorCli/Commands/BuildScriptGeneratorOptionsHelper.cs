// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class BuildScriptGeneratorOptionsHelper
    {
        public static void ConfigureBuildScriptGeneratorOptions(
            BuildScriptGeneratorOptions options,
            string sourceDir = null,
            string destinationDir = null,
            string intermediateDir = null,
            string manifestDir = null,
            string platform = null,
            string platformVersion = null,
            bool? shouldPackage = null,
            bool? compressDestinationDir = null,
            string[] requiredOsPackages = null,
            string appType = null,
            string buildCommandsFileName = null,
            bool? scriptOnly = null,
            string[] properties = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.SourceDir = string.IsNullOrEmpty(sourceDir)
                ? Directory.GetCurrentDirectory() : Path.GetFullPath(sourceDir);
            options.PlatformName = platform;
            options.PlatformVersion = platformVersion;

            if (!string.IsNullOrEmpty(destinationDir))
            {
                options.DestinationDir = Path.GetFullPath(destinationDir);
            }

            if (!string.IsNullOrEmpty(intermediateDir))
            {
                options.IntermediateDir = Path.GetFullPath(intermediateDir);
            }

            if (!string.IsNullOrEmpty(manifestDir))
            {
                options.ManifestDir = Path.GetFullPath(manifestDir);
            }

            if (shouldPackage.HasValue)
            {
                options.ShouldPackage = shouldPackage.Value;
            }

            if (compressDestinationDir.HasValue)
            {
                options.CompressDestinationDir = compressDestinationDir.Value;
            }

            options.RequiredOsPackages = requiredOsPackages;

            if (!string.IsNullOrEmpty(appType))
            {
                options.AppType = appType.Trim();
            }

            if (!string.IsNullOrEmpty(buildCommandsFileName))
            {
                options.BuildCommandsFileName = buildCommandsFileName.Trim();
            }

            if (scriptOnly.HasValue)
            {
                options.ScriptOnly = scriptOnly.Value;
            }

            options.Properties = ProcessProperties(properties);
        }

        // To enable testing
        internal static IDictionary<string, string> ProcessProperties(string[] properties)
        {
            var propertyList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (properties == null)
            {
                return propertyList;
            }

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