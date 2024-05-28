// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.CommandLine;
using System.CommandLine.Binding;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class BuildCommandBinder : BuildCommandBaseBinder<BuildCommandProperty>
    {
        public BuildCommandBinder(
            Option<string> languageName,
            Option<string> languageVersion,
            Option<string> intermediateDir,
            Option<string> destinationDir,
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
            this.LanguageName = languageName;
            this.LanguageVersion = languageVersion;
            this.IntermediateDir = intermediateDir;
            this.DestinationDir = destinationDir;
            this.ManifestDir = manifestDir;
        }

        private Option<string> LanguageName { get; set; }

        private Option<string> LanguageVersion { get; set; }

        private Option<string> IntermediateDir { get; set; }

        private Option<string> DestinationDir { get; set; }

        private Option<string> ManifestDir { get; set; }

        protected override BuildCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new BuildCommandProperty
            {
                LanguageName = bindingContext.ParseResult.GetValueForOption(this.LanguageName),
                LanguageVersion = bindingContext.ParseResult.GetValueForOption(this.LanguageVersion),
                IntermediateDir = bindingContext.ParseResult.GetValueForOption(this.IntermediateDir),
                DestinationDir = bindingContext.ParseResult.GetValueForOption(this.DestinationDir),
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
