using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class RunScriptCommandBinder : CommandBaseBinder<RunScriptCommandProperty>
    {
        private Argument<string> appDirArgument;
        private Option<string> platformNameOption;
        private Option<string> platformVersionOption;
        private Option<string> outputOption;

        public RunScriptCommandBinder(
            Argument<string> appDirArgument,
            Option<string> platformNameOption,
            Option<string> platformVersionOption,
            Option<string> outputOption,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.appDirArgument = appDirArgument;
            this.platformNameOption = platformNameOption;
            this.platformVersionOption = platformVersionOption;
            this.outputOption = outputOption;
        }

        protected override RunScriptCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new RunScriptCommandProperty
            {
                AppDir = bindingContext.ParseResult.GetValueForArgument(this.appDirArgument),
                PlatformName = bindingContext.ParseResult.GetValueForOption(this.platformNameOption),
                PlatformVersion = bindingContext.ParseResult.GetValueForOption(this.platformVersionOption),
                OutputPath = bindingContext.ParseResult.GetValueForOption(this.outputOption),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.logPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.debugMode),
            };
    }
}
