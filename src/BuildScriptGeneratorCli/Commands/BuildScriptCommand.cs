// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Name, Description = "Generate build script to standard output.")]
    internal class BuildScriptCommand : CommandBase
    {
        public const string Name = "build-script";

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

        [Option("--package", CommandOptionType.NoValue,
            Description = "Package the built sources into a platform-specific format.")]
        public bool ShouldPackage { get; set; }

        [Option("--os-requirements", CommandOptionType.SingleValue,
            Description = "Comma-separated list of operating system packages that will be installed (using apt-get) before building the application.")]
        public string OsRequirements { get; set; }

        [Option(
            OptionTemplates.Property,
            CommandOptionType.MultipleValue,
            Description = "Additional information used by this tool to generate and run build scripts.")]
        public string[] Properties { get; set; }

        [Option(
            "--output",
            CommandOptionType.SingleValue,
            Description = "The path that the build script will be written to. If not specified, the result will be written to STDOUT.")]
        public string OutputPath { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var scriptGenerator = new BuildScriptGenerator(
                serviceProvider,
                console,
                checkerMessageSink: null,
                operationId: null);

            if (!scriptGenerator.TryGenerateScript(out var generatedScript))
            {
                return ProcessConstants.ExitFailure;
            }

            if (string.IsNullOrEmpty(OutputPath))
            {
                console.WriteLine(generatedScript);
            }
            else
            {
                File.WriteAllText(OutputPath, generatedScript);
                console.WriteLine($"Script written to '{OutputPath}'");

                // Try making the script executable
                ProcessHelper.TrySetExecutableMode(OutputPath);
            }

            return ProcessConstants.ExitSuccess;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;

            if (!Directory.Exists(options.SourceDir))
            {
                console.WriteErrorLine($"Could not find the source directory '{options.SourceDir}'.");
                return false;
            }

            // Invalid to specify language version without language name
            if (string.IsNullOrEmpty(options.PlatformName) && !string.IsNullOrEmpty(options.PlatformVersion))
            {
                console.WriteErrorLine("Cannot use language version without specifying language name also.");
                return false;
            }

            return true;
        }

        internal override void ConfigureBuildScriptGeneratorOptions(BuildScriptGeneratorOptions options)
        {
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceDir: SourceDir,
                destinationDir: null,
                intermediateDir: null,
                manifestDir: null,
                platform: PlatformName,
                platformVersion: PlatformVersion,
                shouldPackage: ShouldPackage,
                requiredOsPackages: string.IsNullOrWhiteSpace(OsRequirements) ? null : OsRequirements.Split(','),
                scriptOnly: true,
                properties: Properties);
        }
    }
}