using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class ExecCommandBinder : CommandBaseBinder<ExecCommandProperty>
    {
        private Option<string> execSourceDirOption;
        private Argument<string> commandArgument;

        public ExecCommandBinder(
            Option<string> execSourceDirOption,
            Argument<string> commandArgument,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.execSourceDirOption = execSourceDirOption;
            this.commandArgument = commandArgument;
        }

        protected override ExecCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new ExecCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForOption(this.execSourceDirOption),
                Command = bindingContext.ParseResult.GetValueForArgument(this.commandArgument),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.debugMode),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.logPath),
            };
    }
}
