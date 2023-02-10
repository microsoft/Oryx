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
        private Option<string> layersDirOption;
        private Option<string> platformDirOption;
        private Option<string> planPathOption;
        private Option<string> languageName;
        private Option<string> languageVersion;
        private Option<string> intermediateDir;
        private Option<string> output;
        private Option<string> manifestDir;

        public BuildpackBuildCommandBinder(
            Option<string> layersDirOption,
            Option<string> platformDirOption,
            Option<string> planPathOption,
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
            Option<bool> debugMod)
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
                debugMod)
        {
            this.layersDirOption = layersDirOption;
            this.platformDirOption = platformDirOption;
            this.planPathOption = planPathOption;
            this.languageName = languageName;
            this.languageVersion = languageVersion;
            this.intermediateDir = intermediateDir;
            this.output = output;
            this.manifestDir = manifestDir;
        }

        protected override BuildpackBuildCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new BuildpackBuildCommandProperty
            {
                LayersDir = bindingContext.ParseResult.GetValueForOption(this.layersDirOption),
                PlatformDir = bindingContext.ParseResult.GetValueForOption(this.platformDirOption),
                PlanPath = bindingContext.ParseResult.GetValueForOption(this.planPathOption),
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
