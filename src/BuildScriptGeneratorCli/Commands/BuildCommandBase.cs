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
    }
}
