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
    public class DetectCommandBinder : CommandBaseBinder<DetectCommandProperty>
    {
        private Argument<string> sourceDirArgument;
        private Option<string> outputFormatOption;

        public DetectCommandBinder(
            Argument<string> sourceDirArgument,
            Option<string> outputFormatOption,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.sourceDirArgument = sourceDirArgument;
            this.outputFormatOption = outputFormatOption;
        }

        protected override DetectCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new DetectCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForArgument(this.sourceDirArgument),
                OutputFormat = bindingContext.ParseResult.GetValueForOption(this.outputFormatOption),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.logPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.debugMode),
            };
    }
}
