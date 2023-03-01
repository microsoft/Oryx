// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class BuildpackBuildCommandBinder : BuildCommandBaseBinder<BuildpackBuildCommandProperty>
    {
        private Option<string> layersDir;
        private Option<string> platformDir;
        private Option<string> planPath;
        private Option<string> languageName;
        private Option<string> languageVersion;
        private Option<string> intermediateDir;
        private Option<string> output;
        private Option<string> manifestDir;

        public BuildpackBuildCommandBinder(
            Option<string> layersDir,
            Option<string> platformDir,
            Option<string> planPath,
            Option<string> languageName,
            Option<string> languageVersion,
            Option<string> intermediateDir,
            Option<string> output,
            Option<string> manifestDir,
            Argument<string> sourceDir,
            Option<string> platform,
            Option<string> platformVersion,
            Option<bool> package,
            Option<string> osRequirements,
            Option<string> appType,
            Option<string> buildCommandFile,
            Option<bool> compressDestinationDir,
            Option<string[]> property,
            Option<string> dynamicInstallRootDir,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(
                sourceDir,
                platform,
                platformVersion,
                package,
                osRequirements,
                appType,
                buildCommandFile,
                compressDestinationDir,
                property,
                dynamicInstallRootDir,
                logPath,
                debugMode)
        {
            this.layersDir = layersDir;
            this.platformDir = platformDir;
            this.planPath = planPath;
            this.languageName = languageName;
            this.languageVersion = languageVersion;
            this.intermediateDir = intermediateDir;
            this.output = output;
            this.manifestDir = manifestDir;
        }

        protected override BuildpackBuildCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new BuildpackBuildCommandProperty
            {
                LayersDir = bindingContext.ParseResult.GetValueForOption(this.layersDir),
                PlatformDir = bindingContext.ParseResult.GetValueForOption(this.platformDir),
                PlanPath = bindingContext.ParseResult.GetValueForOption(this.planPath),
                LanguageName = bindingContext.ParseResult.GetValueForOption(this.languageName),
                LanguageVersion = bindingContext.ParseResult.GetValueForOption(this.languageVersion),
                IntermediateDir = bindingContext.ParseResult.GetValueForOption(this.intermediateDir),
                DestinationDir = bindingContext.ParseResult.GetValueForOption(this.output),
                ManifestDir = bindingContext.ParseResult.GetValueForOption(this.manifestDir),
                SourceDir = bindingContext.ParseResult.GetValueForArgument(this.SourceDir),
                PlatformName = bindingContext.ParseResult.GetValueForOption(this.Platform),
                PlatformVersion = bindingContext.ParseResult.GetValueForOption(this.PlatformVersion),
                ShouldPackage = bindingContext.ParseResult.GetValueForOption(this.Package),
                OsRequirements = bindingContext.ParseResult.GetValueForOption(this.OsRequirements),
                AppType = bindingContext.ParseResult.GetValueForOption(this.AppType),
                BuildCommandsFileName = bindingContext.ParseResult.GetValueForOption(this.BuildCommandFile),
                CompressDestinationDir = bindingContext.ParseResult.GetValueForOption(this.CompressDestinationDir),
                Properties = bindingContext.ParseResult.GetValueForOption(this.Property),
                DynamicInstallRootDir = bindingContext.ParseResult.GetValueForOption(this.DynamicInstallRootDir),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
