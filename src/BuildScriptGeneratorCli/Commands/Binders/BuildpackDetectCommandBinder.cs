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
    public class BuildpackDetectCommandBinder : CommandBaseBinder<BuildpackDetectCommandProperty>
    {
        private Argument<string> sourceDir;
        private Option<string> platformDir;
        private Option<string> planPath;

        public BuildpackDetectCommandBinder(
            Argument<string> sourceDir,
            Option<string> platformDir,
            Option<string> planPath,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.sourceDir = sourceDir;
            this.platformDir = platformDir;
            this.planPath = planPath;
        }

        protected override BuildpackDetectCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new BuildpackDetectCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForArgument(this.sourceDir),
                PlatformDir = bindingContext.ParseResult.GetValueForOption(this.platformDir),
                PlanPath = bindingContext.ParseResult.GetValueForOption(this.planPath),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
