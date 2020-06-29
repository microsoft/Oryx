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
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Name, Description = "Generate build script to standard output.")]
    internal class BuildScriptCommand : BuildCommandBase
    {
        public const string Name = "build-script";

        [Option(
            "--output",
            CommandOptionType.SingleValue,
            Description = "The path that the build script will be written to. " +
                          "If not specified, the result will be written to STDOUT.")]
        public string OutputPath { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var scriptGenerator = new BuildScriptGenerator(
                serviceProvider,
                console,
                checkerMessageSink: null,
                operationId: null);

            if (!scriptGenerator.TryGenerateScript(out var generatedScript, out var exception))
            {
                if (exception != null)
                {
                    return ProcessExitCodeHelper.GetExitCodeForException(exception);
                }

                return ProcessConstants.ExitFailure;
            }

            if (string.IsNullOrEmpty(OutputPath))
            {
                console.WriteLine(generatedScript);
            }
            else
            {
                OutputPath.SafeWriteAllText(generatedScript);
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

            // Invalid to specify platform version without platform name
            if (string.IsNullOrEmpty(options.PlatformName) && !string.IsNullOrEmpty(options.PlatformVersion))
            {
                console.WriteErrorLine("Cannot use platform version without platform name also.");
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
                requiredOsPackages: string.IsNullOrWhiteSpace(SystemPackages) ? null : SystemPackages.Split(','),
                scriptOnly: true,
                properties: Properties);
        }
    }
}