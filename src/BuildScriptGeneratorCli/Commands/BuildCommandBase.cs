// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using McMaster.Extensions.CommandLineUtils;

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
            OptionTemplates.SystemPackages,
            CommandOptionType.SingleValue,
            Description = "Comma-separated list of operating system packages that will be installed (using apt-get) " +
            "before building the application.")]
        public string SystemPackages { get; set; }

        [Option(
            OptionTemplates.Property,
            CommandOptionType.MultipleValue,
            Description = "Additional information used by this tool to generate and run build scripts.")]
        public string[] Properties { get; set; }
    }
}
