// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class BuildpackBuildCommand : BuildCommand
    {
        public BuildpackBuildCommand()
        {
        }

        public BuildpackBuildCommand(BuildpackBuildCommandProperty input)
        {
            this.LayersDir = input.LayersDir;
            this.PlatformDir = input.PlatformDir;
            this.PlanPath = input.PlanPath;
            this.IntermediateDir = input.IntermediateDir;
            this.LanguageName = input.LanguageName;
            this.LanguageVersion = input.LanguageVersion;
            this.DestinationDir = input.DestinationDir;
            this.ManifestDir = input.ManifestDir;
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

        public string LayersDir { get; set; }

        public string PlatformDir { get; set; }

        public string PlanPath { get; set; }

        public static new Command Export(IConsole console)
        {
            // Options for BuildpackBuildCommand
            var layersDirOption = new Option<string>("--layers-dir", "Layers directory path.");
            var platformDirOption = new Option<string>("--platform-dir", "Platform directory path.");
            var planPathOption = new Option<string>("--plan-path", "Build plan TOML path.");

            // Options from BuildCommand
            var logOption = new Option<string>(OptionTemplates.Log, OptionTemplates.LogDescription);
            var debugOption = new Option<bool>(OptionTemplates.Debug, OptionTemplates.DebugDescription);
            var sourceDirArgument = new Argument<string>("sourceDir", "The source directory.");
            var platformOption = new Option<string>(OptionTemplates.Platform, OptionTemplates.PlatformDescription);
            var platformVersionOption = new Option<string>(OptionTemplates.PlatformVersion, OptionTemplates.PlatformVersionDescription);
            var packageOption = new Option<bool>(OptionTemplates.Package, OptionTemplates.PackageDescription);
            var osReqOption = new Option<string>(OptionTemplates.OsRequirements, OptionTemplates.OsRequirementsDescription);
            var appTypeOption = new Option<string>(OptionTemplates.AppType, OptionTemplates.AppTypeDescription);
            var buildCommandFileNameOption = new Option<string>(OptionTemplates.BuildCommandsFileName, OptionTemplates.BuildCommandsFileNameDescription);
            var compressDestDirOption = new Option<bool>(OptionTemplates.CompressDestinationDir, OptionTemplates.CompressDestinationDirDescription);
            var propertyOption = new Option<string[]>(aliases: new[] { "-p", OptionTemplates.Property }, OptionTemplates.PropertyDescription);
            var dynamicInstallRootDirOption = new Option<string>(OptionTemplates.DynamicInstallRootDir, OptionTemplates.DynamicInstallRootDirDescription);

            // Hiding Language Option because it is obselete
            var languageOption = new Option<string>(aliases: new[] { "-l", OptionTemplates.Language }, OptionTemplates.LanguageDescription);
            languageOption.IsHidden = true;

            // Hiding LanguageVer Option because it is obselete
            var languageVerOption = new Option<string>(OptionTemplates.LanguageVersion, OptionTemplates.LanguageVersionDescription);
            languageVerOption.IsHidden = true;

            var intermediateDirOption = new Option<string>(aliases: new[] { "-i", OptionTemplates.IntermediateDir }, OptionTemplates.IntermediateDirDescription);
            var outputOption = new Option<string>(aliases: new[] { "-o", OptionTemplates.Output }, OptionTemplates.OutputDescription);
            var manifestDirOption = new Option<string>(OptionTemplates.ManifestDir, OptionTemplates.ManifestDirDescription);

            var command = new Command("buildpack-build", "Build an app in the current working directory (for use in a Buildpack).")
            {
                layersDirOption,
                platformDirOption,
                planPathOption,
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
                languageOption,
                languageVerOption,
                intermediateDirOption,
                outputOption,
                manifestDirOption,
            };

            command.SetHandler(
                (prop) =>
                {
                    var buildpackBuildCommand = new BuildpackBuildCommand(prop);
                    buildpackBuildCommand.OnExecute(console);
                },
                new BuildpackBuildCommandBinder(
                    layersDirOption,
                    platformDirOption,
                    planPathOption,
                    languageOption,
                    languageVerOption,
                    intermediateDirOption,
                    outputOption,
                    manifestDirOption,
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

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var result = base.IsValidInput(serviceProvider, console);
            var logger = serviceProvider.GetRequiredService<ILogger<BuildpackBuildCommand>>();

            if (!string.IsNullOrWhiteSpace(this.LayersDir))
            {
                this.LayersDir = Path.GetFullPath(this.LayersDir);
                if (!Directory.Exists(this.LayersDir))
                {
                    logger.LogError("Could not find provided layers directory.");
                    console.Error.WriteLine($"Could not find layers directory '{this.LayersDir}'.");
                    result = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(this.PlanPath))
            {
                this.PlanPath = Path.GetFullPath(this.PlanPath);
                if (!File.Exists(this.PlanPath))
                {
                    logger?.LogError("Could not find build plan file {planPath}", this.PlanPath);
                    console.Error.WriteLine($"Could not find build plan file '{this.PlanPath}'.");
                    result = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(this.PlatformDir))
            {
                this.PlatformDir = Path.GetFullPath(this.PlatformDir);
                if (!Directory.Exists(this.PlatformDir))
                {
                    logger?.LogError("Could not find platform directory {platformDir}", this.PlatformDir);
                    console.Error.WriteLine($"Could not find platform directory '{this.PlatformDir}'.");
                    result = false;
                }
            }

            return result;
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            return base.Execute(serviceProvider, console);
        }
    }
}
