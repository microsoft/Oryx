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
        public ExecCommandBinder(
            Option<string> execSourceDir,
            Argument<string> command,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            // SourceDir matches with the name of the property
            // execSourceDir matches with the name of the option
            this.SourceDir = execSourceDir;
            this.Command = command;
        }

        private Option<string> SourceDir { get; set; }

        private Argument<string> Command { get; set; }

        protected override ExecCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new ExecCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForOption(this.SourceDir),
                Command = bindingContext.ParseResult.GetValueForArgument(this.Command),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
                LogPath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
            };
    }
}
