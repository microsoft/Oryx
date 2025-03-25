// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class BuildScriptCommand : BuildCommandBase
    {
        public const string Name = "build-script";
        public const string Description = "Generate build script to standard output.";

        public BuildScriptCommand()
        {
        }

        public BuildScriptCommand(BuildScriptCommandProperty input)
        {
            this.OutputPath = input.OutputPath;
            this.SourceDir = input.SourceDir;
            this.PlatformName = input.Platform;
            this.PlatformVersion = input.PlatformVersion;
            this.ShouldPackage = input.ShouldPackage;
            this.OsRequirements = input.OsRequirements;
            this.AppType = input.AppType;
            this.BuildCommandsFileName = input.BuildCommandFile;
            this.CompressDestinationDir = input.CompressDestinationDir;
            this.Properties = input.Property;
            this.DynamicInstallRootDir = input.DynamicInstallRootDir;
            this.LogFilePath = input.LogPath;
            this.DebugMode = input.DebugMode;
        }

        public string OutputPath { get; set; }

        public static Command Export(IConsole console)
        {
            var logOption = new Option<string>(OptionArgumentTemplates.Log, OptionArgumentTemplates.LogDescription);
            var debugOption = new Option<bool>(OptionArgumentTemplates.Debug, OptionArgumentTemplates.DebugDescription);
            var sourceDirArgument = new Argument<string>(
                name: OptionArgumentTemplates.SourceDir,
                description: OptionArgumentTemplates.SourceDirDescription,
                getDefaultValue: () => Directory.GetCurrentDirectory());
            var platformOption = new Option<string>(OptionArgumentTemplates.Platform, OptionArgumentTemplates.PlatformDescription);
            var platformVersionOption = new Option<string>(OptionArgumentTemplates.PlatformVersion, OptionArgumentTemplates.PlatformVersionDescription);
            var packageOption = new Option<bool>(OptionArgumentTemplates.Package, OptionArgumentTemplates.PackageDescription);
            var osReqOption = new Option<string>(OptionArgumentTemplates.OsRequirements, OptionArgumentTemplates.OsRequirementsDescription);
            var appTypeOption = new Option<string>(OptionArgumentTemplates.AppType, OptionArgumentTemplates.AppTypeDescription);
            var buildCommandFileNameOption = new Option<string>(OptionArgumentTemplates.BuildCommandsFileName, OptionArgumentTemplates.BuildCommandsFileNameDescription);
            var compressDestDirOption = new Option<bool>(OptionArgumentTemplates.CompressDestinationDir, OptionArgumentTemplates.CompressDestinationDirDescription);
            var propertyOption = new Option<string[]>(OptionArgumentTemplates.Property, OptionArgumentTemplates.PropertyDescription);
            var dynamicInstallRootDirOption = new Option<string>(OptionArgumentTemplates.DynamicInstallRootDir, OptionArgumentTemplates.DynamicInstallRootDirDescription);
            var buildScriptOutputOption = new Option<string>(
                aliases: OptionArgumentTemplates.Output,
                description: OptionArgumentTemplates.BuildScriptOutputDescription);

            var command = new Command(Name, Description)
            {
                logOption,
                debugOption,
                sourceDirArgument,
                platformOption,
                platformVersionOption,
                packageOption,
                osReqOption,
                appTypeOption,
                buildCommandFileNameOption,
                compressDestDirOption,
                propertyOption,
                dynamicInstallRootDirOption,
                buildScriptOutputOption,
            };

            command.SetHandler(
                (prop) =>
                {
                    var buildScriptCommand = new BuildScriptCommand(prop);
                    return Task.FromResult(buildScriptCommand.OnExecute(console));
                },
                new BuildScriptCommandBinder(
                    outputPath: buildScriptOutputOption,
                    sourceDir: sourceDirArgument,
                    platform: platformOption,
                    platformVersion: platformVersionOption,
                    package: packageOption,
                    osRequirements: osReqOption,
                    appType: appTypeOption,
                    buildCommandFile: buildCommandFileNameOption,
                    compressDestinationDir: compressDestDirOption,
                    property: propertyOption,
                    dynamicInstallRootDir: dynamicInstallRootDirOption,
                    logPath: logOption,
                    debugMode: debugOption));
            return command;
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var scriptGenerator = new BuildScriptGenerator(
                serviceProvider,
                console,
                checkerMessageSink: null,
                operationId: null);

            // TODO check if changes needed here as well?
            if (!scriptGenerator.TryGenerateScript(out var generatedScript, out var exception))
            {
                if (exception != null)
                {
                    return ProcessExitCodeHelper.GetExitCodeForException(exception);
                }

                return ProcessConstants.ExitFailure;
            }

            if (string.IsNullOrEmpty(this.OutputPath))
            {
                console.WriteLine(generatedScript);
            }
            else
            {
                this.OutputPath.SafeWriteAllText(generatedScript);
                console.WriteLine($"Script written to '{this.OutputPath}'");

                // Try making the script executable
                ProcessHelper.TrySetExecutableMode(this.OutputPath);
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
                sourceDir: this.SourceDir,
                destinationDir: null,
                intermediateDir: null,
                manifestDir: null,
                platform: this.PlatformName,
                platformVersion: this.PlatformVersion,
                shouldPackage: this.ShouldPackage,
                requiredOsPackages: string.IsNullOrWhiteSpace(this.OsRequirements) ? null : this.OsRequirements.Split(','),
                appType: this.AppType,
                scriptOnly: true,
                properties: this.Properties);
        }

        internal override IServiceProvider TryGetServiceProvider(IConsole console)
        {
            // Don't use the IConsole instance in this method -- override this method in the command
            // and pass IConsole through to ServiceProviderBuilder to write to the output.
            var serviceProviderBuilder = new ServiceProviderBuilder(this.LogFilePath)
                .ConfigureServices(services =>
                {
                    var configuration = new ConfigurationBuilder()
                        .AddEnvironmentVariables()
                        .Build();

                    services.AddSingleton<IConfiguration>(configuration);
                })
                .ConfigureScriptGenerationOptions(opts =>
                {
                    this.ConfigureBuildScriptGeneratorOptions(opts);

                    opts.DebianFlavor = this.ResolveOsType(opts, console);
                    opts.ImageType = this.ResolveImageType(opts, console);
                });
            return serviceProviderBuilder.Build();
        }
    }
}