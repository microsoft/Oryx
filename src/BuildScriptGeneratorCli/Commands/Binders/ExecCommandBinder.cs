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
    public class ExecCommandBinder : CommandBaseBinder<ExecCommandProperty>
    {
        private Option<string> execSourceDir;
        private Argument<string> command;

        public ExecCommandBinder(
            Option<string> execSourceDir,
            Argument<string> command,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.execSourceDir = execSourceDir;
            this.command = command;
        }

        protected override ExecCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new ExecCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForOption(this.execSourceDir),
                Command = bindingContext.ParseResult.GetValueForArgument(this.command),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
            };
    }
}
