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
            this.LayersDir = layersDir;
            this.PlatformDir = platformDir;
            this.PlanPath = planPath;
            this.LanguageName = languageName;
            this.LanguageVersion = languageVersion;
            this.IntermediateDir = intermediateDir;
            this.Output = output;
            this.ManifestDir = manifestDir;
        }

        private Option<string> LayersDir { get; set; }

        private Option<string> PlatformDir { get; set; }

        private Option<string> PlanPath { get; set; }

        private Option<string> LanguageName { get; set; }

        private Option<string> LanguageVersion { get; set; }

        private Option<string> IntermediateDir { get; set; }

        private Option<string> Output { get; set; }

        private Option<string> ManifestDir { get; set; }

        protected override BuildpackBuildCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new BuildpackBuildCommandProperty
            {
                LayersDir = bindingContext.ParseResult.GetValueForOption(this.LayersDir),
                PlatformDir = bindingContext.ParseResult.GetValueForOption(this.PlatformDir),
                PlanPath = bindingContext.ParseResult.GetValueForOption(this.PlanPath),
                LanguageName = bindingContext.ParseResult.GetValueForOption(this.LanguageName),
                LanguageVersion = bindingContext.ParseResult.GetValueForOption(this.LanguageVersion),
                IntermediateDir = bindingContext.ParseResult.GetValueForOption(this.IntermediateDir),
                DestinationDir = bindingContext.ParseResult.GetValueForOption(this.Output),
                ManifestDir = bindingContext.ParseResult.GetValueForOption(this.ManifestDir),
                SourceDir = bindingContext.ParseResult.GetValueForArgument(this.SourceDir),
                Platform = bindingContext.ParseResult.GetValueForOption(this.Platform),
                PlatformVersion = bindingContext.ParseResult.GetValueForOption(this.PlatformVersion),
                ShouldPackage = bindingContext.ParseResult.GetValueForOption(this.ShouldPackage),
                OsRequirements = bindingContext.ParseResult.GetValueForOption(this.OsRequirements),
                AppType = bindingContext.ParseResult.GetValueForOption(this.AppType),
                BuildCommandFile = bindingContext.ParseResult.GetValueForOption(this.BuildCommandFile),
                CompressDestinationDir = bindingContext.ParseResult.GetValueForOption(this.CompressDestinationDir),
                Property = bindingContext.ParseResult.GetValueForOption(this.Property),
                DynamicInstallRootDir = bindingContext.ParseResult.GetValueForOption(this.DynamicInstallRootDir),
                LogPath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
