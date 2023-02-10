// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class BuildScriptCommand : BuildCommandBase
    {
        public BuildScriptCommand()
        {
        }

        public BuildScriptCommand(BuildScriptCommandProperty input)
        {
            this.OutputPath = input.OutputPath;
            this.SourceDir = input.SourceDir;
            this.PlatformName = input.PlatformName;
            this.PlatformVersion = input.PlatformVersion;
            this.ShouldPackage = input.ShouldPackage;
            this.OsRequirements = input.OsRequirements;
            this.AppType = input.AppType;
            this.BuildCommandsFileName = input.BuildCommandsFileName;
            this.CompressDestinationDir = input.CompressDestinationDir;
            this.Properties = input.Properties;
            this.DynamicInstallRootDir = input.DynamicInstallRootDir;
            this.LogFilePath = input.LogFilePath;
            this.DebugMode = input.DebugMode;
        }

        public string OutputPath { get; set; }

        public static Command Export()
        {
            var logOption = new Option<string>(OptionTemplates.Log, OptionTemplates.LogDescription);
            var debugOption = new Option<bool>(OptionTemplates.Debug, OptionTemplates.DebugDescription);
            var sourceDirArgument = new Argument<string>("SourceDir", "The source directory.");
            var platformOption = new Option<string>(OptionTemplates.Platform, OptionTemplates.PlatformDescription);
            var platformVersionOption = new Option<string>(OptionTemplates.PlatformVersion, OptionTemplates.PlatformVersionDescription);
            var packageOption = new Option<bool>(OptionTemplates.Package, OptionTemplates.PackageDescription);
            var osReqOption = new Option<string>(OptionTemplates.OsRequirements, OptionTemplates.OsRequirementsDescription);
            var appTypeOption = new Option<string>(OptionTemplates.AppType, OptionTemplates.AppTypeDescription);
            var buildCommandFileNameOption = new Option<string>(OptionTemplates.BuildCommandsFileName, OptionTemplates.BuildCommandsFileNameDescription);
            var compressDestDirOption = new Option<bool>(OptionTemplates.CompressDestinationDir, OptionTemplates.CompressDestinationDirDescription);
            var propertyOption = new Option<string[]>(aliases: new[] { "-p", OptionTemplates.Property }, OptionTemplates.PropertyDescription);
            var dynamicInstallRootDirOption = new Option<string>(OptionTemplates.DynamicInstallRootDir, OptionTemplates.DynamicInstallRootDirDescription);
            var buildScriptOutputOption = new Option<string>(
                name: "--output",
                description: "The path that the build script will be written to. " +
                          "If not specified, the result will be written to STDOUT.");

            var command = new Command("build-script", "Generate build script to standard output.")
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
                    buildScriptCommand.OnExecute();
                },
                new BuildScriptCommandBinder(
                    buildScriptOutputOption,
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
                    logOption,
                    debugOption));
            return command;
        }

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
                console.Error.WriteLine($"Could not find the source directory '{options.SourceDir}'.");
                return false;
            }

            // Invalid to specify platform version without platform name
            if (string.IsNullOrEmpty(options.PlatformName) && !string.IsNullOrEmpty(options.PlatformVersion))
            {
                console.Error.WriteLine("Cannot use platform version without platform name also.");
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