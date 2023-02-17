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
                SourceDir = bindingContext.ParseResult.GetValueForArgument(this.sourceDir),
                PlatformName = bindingContext.ParseResult.GetValueForOption(this.platform),
                PlatformVersion = bindingContext.ParseResult.GetValueForOption(this.platformVersion),
                ShouldPackage = bindingContext.ParseResult.GetValueForOption(this.package),
                OsRequirements = bindingContext.ParseResult.GetValueForOption(this.osRequirements),
                AppType = bindingContext.ParseResult.GetValueForOption(this.appType),
                BuildCommandsFileName = bindingContext.ParseResult.GetValueForOption(this.buildCommandFile),
                CompressDestinationDir = bindingContext.ParseResult.GetValueForOption(this.compressDestinationDir),
                Properties = bindingContext.ParseResult.GetValueForOption(this.property),
                DynamicInstallRootDir = bindingContext.ParseResult.GetValueForOption(this.dynamicInstallRootDir),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.logPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.debugMode),
            };
    }
}
