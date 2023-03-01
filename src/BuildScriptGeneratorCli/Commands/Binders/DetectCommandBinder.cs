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
        private Argument<string> sourceDir;
        private Option<string> outputFormat;

        public DetectCommandBinder(
            Argument<string> sourceDir,
            Option<string> outputFormat,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.sourceDir = sourceDir;
            this.outputFormat = outputFormat;
        }

        protected override DetectCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new DetectCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForArgument(this.sourceDir),
                OutputFormat = bindingContext.ParseResult.GetValueForOption(this.outputFormat),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
