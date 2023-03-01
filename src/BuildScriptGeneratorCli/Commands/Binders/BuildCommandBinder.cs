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
        private Option<string> languageName;
        private Option<string> languageVersion;
        private Option<string> intermediateDir;
        private Option<string> output;
        private Option<string> manifestDir;

        public BuildCommandBinder(
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
            this.languageName = languageName;
            this.languageVersion = languageVersion;
            this.intermediateDir = intermediateDir;
            this.output = output;
            this.manifestDir = manifestDir;
        }

        protected override BuildCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new BuildCommandProperty
            {
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
