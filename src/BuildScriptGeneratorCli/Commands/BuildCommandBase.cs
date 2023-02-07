// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal abstract class BuildCommandBase : CommandBase
    {
        public string SourceDir { get; set; }

        public string PlatformName { get; set; }

        public string PlatformVersion { get; set; }

        public bool ShouldPackage { get; set; }

        public string OsRequirements { get; set; }

        public string AppType { get; set; }

        public string BuildCommandsFileName { get; set; }

        public bool CompressDestinationDir { get; set; }

        public string[] Properties { get; set; }

        public string DynamicInstallRootDir { get; set; }

        protected string ResolveOsType(BuildScriptGeneratorOptions options, IConsole console)
        {
            // For debian flavor, we first check for existence of an environment variable
            // which contains the os type. If this does not exist, parse the
            // FilePaths.OsTypeFileName file for the correct flavor
            if (string.IsNullOrWhiteSpace(options.DebianFlavor))
            {
                var parsedOsType = ParseOsTypeFile();
                if (parsedOsType != null)
                {
                    if (this.DebugMode)
                    {
                        console.WriteLine(
                            $"Warning: DEBIAN_FLAVOR environment variable not found. " +
                            $"Falling back to debian flavor in the {FilePaths.OsTypeFileName} file.");
                    }

                    return parsedOsType;
                }

                // If we cannot resolve the debian flavor, error out as we will not be able to determine
                // the correct SDKs to pull
                var errorMessage = $"Error: Image debian flavor not found in DEBIAN_FLAVOR environment variable or the " +
                    $"{Path.Join("/opt", "oryx", FilePaths.OsTypeFileName)} file. Exiting...";
                throw new InvalidUsageException(errorMessage);
            }

            return options.DebianFlavor;
        }

        protected string ResolveImageType(BuildScriptGeneratorOptions options, IConsole console)
        {
            // try to parse image type from file
            // unlike os type, do not fail if image type not found, as it is only used for
            // telemetry purposes
            if (string.IsNullOrWhiteSpace(options.ImageType))
            {
                var parsedImageType = ParseImageTypeFile();
                if (parsedImageType != null)
                {
                    options.ImageType = parsedImageType;
                    if (this.DebugMode)
                    {
                        console.WriteLine($"Parsed image type from file '{FilePaths.ImageTypeFileName}': {options.ImageType}");
                    }
                }
                else
                {
                    if (this.DebugMode)
                    {
                        console.WriteLine($"Warning: '{FilePaths.ImageTypeFileName}' file not found.");
                    }
                }
            }

            return options.ImageType;
        }
    }
}
