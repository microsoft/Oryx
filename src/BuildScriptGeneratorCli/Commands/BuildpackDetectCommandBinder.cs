using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class BuildpackDetectCommandBinder : CommandBaseBinder<BuildpackDetectCommandProperty>
    {
        private Argument<string> sourceDirArgument;
        private Option<string> platformDirOption;
        private Option<string> planPathOption;

        public BuildpackDetectCommandBinder(
            Argument<string> sourceDirArgument,
            Option<string> platformDirOption,
            Option<string> planPathOption,
            Option<string> logPathOption,
            Option<bool> debugMode)
            : base(logPathOption, debugMode)
        {
            this.sourceDirArgument = sourceDirArgument;
            this.platformDirOption = platformDirOption;
            this.planPathOption = planPathOption;
        }

        protected override BuildpackDetectCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new BuildpackDetectCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForArgument(this.sourceDirArgument),
                PlatformDir = bindingContext.ParseResult.GetValueForOption(this.platformDirOption),
                PlanPath = bindingContext.ParseResult.GetValueForOption(this.planPathOption),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.logPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.debugMode),
            };
    }
}
