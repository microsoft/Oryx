// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class BuildpackBuildCommand : BuildCommand
    {
        // Use "new" keyword to avoid hiding the base class's static members
        public new const string Name = "buildpack-build";
        public new const string Description = "Build an app in the current working directory (for use in a Buildpack).";

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

        public string LayersDir { get; set; }

        public string PlatformDir { get; set; }

        public string PlanPath { get; set; }

        public static new Command Export(IConsole console)
        {
            // Options for BuildpackBuildCommand
            var layersDirOption = new Option<string>(OptionArgumentTemplates.LayersDir, OptionArgumentTemplates.LayersDirDescription);
            var platformDirOption = new Option<string>(OptionArgumentTemplates.PlatformDir, OptionArgumentTemplates.PlatformDirDescription);
            var planPathOption = new Option<string>(OptionArgumentTemplates.PlanPath, OptionArgumentTemplates.PlanPathDescription);

            // Options from BuildCommand
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

            // Hiding Language Option because it is obselete
            var languageOption = new Option<string>(OptionArgumentTemplates.Language, OptionArgumentTemplates.LanguageDescription);
            languageOption.IsHidden = true;

            // Hiding LanguageVer Option because it is obselete
            var languageVerOption = new Option<string>(OptionArgumentTemplates.LanguageVersion, OptionArgumentTemplates.LanguageVersionDescription);
            languageVerOption.IsHidden = true;

            var intermediateDirOption = new Option<string>(OptionArgumentTemplates.IntermediateDir, OptionArgumentTemplates.IntermediateDirDescription);
            var outputOption = new Option<string>(OptionArgumentTemplates.Output, OptionArgumentTemplates.OutputDescription);
            var manifestDirOption = new Option<string>(OptionArgumentTemplates.ManifestDir, OptionArgumentTemplates.ManifestDirDescription);

            var command = new Command(Name, Description)
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
                    return Task.FromResult(buildpackBuildCommand.OnExecute(console));
                },
                new BuildpackBuildCommandBinder(
                    layersDir: layersDirOption,
                    platformDir: platformDirOption,
                    planPath: planPathOption,
                    languageName: languageOption,
                    languageVersion: languageVerOption,
                    intermediateDir: intermediateDirOption,
                    output: outputOption,
                    manifestDir: manifestDirOption,
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
                    console.WriteErrorLine($"Could not find layers directory '{this.LayersDir}'.");
                    result = false;
                }
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

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            return base.Execute(serviceProvider, console);
        }
    }
}
