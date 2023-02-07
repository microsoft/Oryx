using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class BuildScriptCommandBinder : BuildCommandBaseBinder<BuildScriptCommandProperty>
    {
        private Option<string> buildScriptOutputOption;

        public BuildScriptCommandBinder(
            Option<string> buildScriptOutputOption,
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
            : base(sourceDir, platform, platformVersion, package, osRequirements, appType, buildCommandFile, compressDestinationDir, property, dynamicInstallRootDir, logPath, debugMod)
        {
            this.buildScriptOutputOption = buildScriptOutputOption;
        }

        protected override BuildScriptCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new BuildScriptCommandProperty
            {
                OutputPath = bindingContext.ParseResult.GetValueForOption(this.buildScriptOutputOption),
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
