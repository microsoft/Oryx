// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class BuildpackDetectCommand : CommandBase
    {
        public const string Name = "buildpack-detect";
        public const string Description =
            "Determine whether Oryx can be applied as a buildpack to an app in the current " +
            "working directory.";

        // CodeDetectFail @ https://github.com/buildpack/lifecycle/blob/master/detector.go
        public const int DetectorFailCode = 100;

        public BuildpackDetectCommand()
        {
        }

        public BuildpackDetectCommand(BuildpackDetectCommandProperty input)
        {
            this.PlatformDir = input.PlatformDir;
            this.SourceDir = input.SourceDir;
            this.PlanPath = input.PlanPath;
            this.LogFilePath = input.LogPath;
            this.DebugMode = input.DebugMode;
        }

        public string SourceDir { get; set; }

        public string PlatformDir { get; set; }

        public string PlanPath { get; set; }

        public static Command Export(IConsole console)
        {
            var sourceDirArgument = new Argument<string>(
                name: OptionArgumentTemplates.SourceDir,
                description: OptionArgumentTemplates.SourceDirDescription,
                getDefaultValue: () => Directory.GetCurrentDirectory());
            var platformDirOption = new Option<string>(OptionArgumentTemplates.PlatformDir, OptionArgumentTemplates.BuildpackDetectPlatformDirDescription);
            var planPathOption = new Option<string>(OptionArgumentTemplates.PlanPath, OptionArgumentTemplates.PlanPathDescription);
            var logFilePathOption = new Option<string>(OptionArgumentTemplates.Log, OptionArgumentTemplates.LogDescription);
            var debugOption = new Option<bool>(OptionArgumentTemplates.Debug, OptionArgumentTemplates.DebugDescription);

            var command = new Command(Name, Description)
            {
                sourceDirArgument,
                platformDirOption,
                planPathOption,
                logFilePathOption,
                debugOption,
            };

            command.SetHandler(
                (prop) =>
                {
                    var buildpackDetectCommand = new BuildpackDetectCommand(prop);
                    return Task.FromResult(buildpackDetectCommand.OnExecute(console));
                },
                new BuildpackDetectCommandBinder(
                    sourceDir: sourceDirArgument,
                    platformDir: platformDirOption,
                    planPath: planPathOption,
                    logPath: logFilePathOption,
                    debugMode: debugOption));

            return command;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var result = true;
            var options = serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
            var logger = serviceProvider.GetService<ILogger<BuildpackDetectCommand>>();

            // Set from ConfigureBuildScriptGeneratorOptions
            if (!Directory.Exists(options.SourceDir))
            {
                logger.LogError("Could not find the source directory.");
                console.WriteErrorLine($"Could not find the source directory '{options.SourceDir}'.");
                result = false;
            }

            if (!string.IsNullOrWhiteSpace(this.PlanPath))
            {
                this.PlanPath = Path.GetFullPath(this.PlanPath);
                if (!File.Exists(this.PlanPath))
                {
                    logger?.LogError("Could not find build plan file {planPath}", this.PlanPath);
                    console.WriteErrorLine($"Could not find build plan file '{this.PlanPath}'.");
                    result = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(this.PlatformDir))
            {
                this.PlatformDir = Path.GetFullPath(this.PlatformDir);
                if (!Directory.Exists(this.PlatformDir))
                {
                    logger?.LogError("Could not find platform directory {platformDir}", this.PlatformDir);
                    console.WriteErrorLine($"Could not find platform directory '{this.PlatformDir}'.");
                    result = false;
                }
            }

            return result;
        }

        internal override void ConfigureBuildScriptGeneratorOptions(BuildScriptGeneratorOptions options)
        {
            BuildScriptGeneratorOptionsHelper.ConfigureBuildScriptGeneratorOptions(
                options,
                sourceDir: this.SourceDir,
                destinationDir: null,
                intermediateDir: null,
                manifestDir: null,
                platform: null,
                platformVersion: null,
                shouldPackage: false,
                requiredOsPackages: null,
                scriptOnly: false,
                properties: null);
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var detector = serviceProvider.GetRequiredService<ICompatiblePlatformDetector>();

            var ctx = BuildScriptGenerator.CreateContext(serviceProvider, operationId: null);
            var compatPlats = detector.GetCompatiblePlatforms(ctx);

            if (compatPlats != null && compatPlats.Any())
            {
                console.WriteLine("Detected platforms:");
                console.WriteLine(string.Join(' ', compatPlats.Select(pair => $"{pair.Key.Name}=\"{pair.Value}\"")));

                // Write the detected platforms into the build plan as TOML
                File.WriteAllLines(this.PlanPath, compatPlats.Select(pair => $"{pair.Key.Name} = {{ version = \"{pair.Value}\" }}"));

                return ProcessConstants.ExitSuccess;
            }

            return DetectorFailCode;
        }
    }
}
