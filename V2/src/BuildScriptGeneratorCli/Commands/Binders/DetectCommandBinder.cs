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
        public DetectCommandBinder(
            Argument<string> sourceDir,
            Option<string> outputFormat,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.SourceDir = sourceDir;
            this.OutputFormat = outputFormat;
        }

        private Argument<string> SourceDir { get; set; }

        private Option<string> OutputFormat { get; set; }

        protected override DetectCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new DetectCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForArgument(this.SourceDir),
                OutputFormat = bindingContext.ParseResult.GetValueForOption(this.OutputFormat),
                LogPath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
