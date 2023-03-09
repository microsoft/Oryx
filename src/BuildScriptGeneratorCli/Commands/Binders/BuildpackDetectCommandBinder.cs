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
        public BuildpackDetectCommandBinder(
            Argument<string> sourceDir,
            Option<string> platformDir,
            Option<string> planPath,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.SourceDir = sourceDir;
            this.PlatformDir = platformDir;
            this.PlanPath = planPath;
        }

        private Argument<string> SourceDir { get; set; }

        private Option<string> PlatformDir { get; set; }

        private Option<string> PlanPath { get; set; }

        protected override BuildpackDetectCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new BuildpackDetectCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForArgument(this.SourceDir),
                PlatformDir = bindingContext.ParseResult.GetValueForOption(this.PlatformDir),
                PlanPath = bindingContext.ParseResult.GetValueForOption(this.PlanPath),
                LogPath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
