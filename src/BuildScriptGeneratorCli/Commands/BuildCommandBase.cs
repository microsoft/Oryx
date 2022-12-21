// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal abstract class BuildCommandBase : CommandBase
    {
        [Argument(0, Description = "The source directory.")]
        [DirectoryExists]
        public string SourceDir { get; set; }

        [Option(
            OptionTemplates.Platform,
            CommandOptionType.SingleValue,
            Description = "The name of the programming platform used in the provided source directory.")]
        public string PlatformName { get; set; }

        [Option(
            OptionTemplates.PlatformVersion,
            CommandOptionType.SingleValue,
            Description = "The version of the programming platform used in the provided source directory.")]
        public string PlatformVersion { get; set; }

        [Option(
            "--package",
            CommandOptionType.NoValue,
            Description = "Package the built sources into a platform-specific format.")]
        public bool ShouldPackage { get; set; }

        [Option(
            "--os-requirements",
            CommandOptionType.SingleValue,
            Description = "Comma-separated list of operating system packages that will be installed (using apt-get) before building the application.")]
        public string OsRequirements { get; set; }

        [Option(
            OptionTemplates.AppType,
            CommandOptionType.SingleValue,
            Description = "Type of application that the source directory has, for example: 'functions' or 'static-sites' etc.")]
        public string AppType { get; set; }

        [Option(
            OptionTemplates.BuildCommandsFileName,
            CommandOptionType.SingleValue,
            Description = "Name of the file where list of build commands will be printed for node and python applications.")]
        public string BuildCommandsFileName { get; set; }

        [Option(
            OptionTemplates.CompressDestinationDir,
            CommandOptionType.NoValue,
            Description = "Compresses the destination directory(excluding the manifest file) into a tarball.")]
        public bool CompressDestinationDir { get; set; }

        [Option(
            OptionTemplates.Property,
            CommandOptionType.MultipleValue,
            Description = "Additional information used by this tool to generate and run build scripts.")]
        public string[] Properties { get; set; }

        [Option(
            OptionTemplates.DynamicInstallRootDir,
            CommandOptionType.SingleValue,
            Description = "Root directory path under which dynamically installed SDKs are created under.")]
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
