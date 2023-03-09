// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.CommandLine;
using System.CommandLine.Binding;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class RunScriptCommandBinder : CommandBaseBinder<RunScriptCommandProperty>
    {
        public RunScriptCommandBinder(
            Argument<string> appDir,
            Option<string> platformName,
            Option<string> platformVersion,
            Option<string> output,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.AppDir = appDir;
            this.PlatformName = platformName;
            this.PlatformVersion = platformVersion;
            this.Output = output;
        }

        private Argument<string> AppDir { get; set; }

        private Option<string> PlatformName { get; set; }

        private Option<string> PlatformVersion { get; set; }

        private Option<string> Output { get; set; }

        protected override RunScriptCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new RunScriptCommandProperty
            {
                AppDir = bindingContext.ParseResult.GetValueForArgument(this.AppDir),
                PlatformName = bindingContext.ParseResult.GetValueForOption(this.PlatformName),
                PlatformVersion = bindingContext.ParseResult.GetValueForOption(this.PlatformVersion),
                Output = bindingContext.ParseResult.GetValueForOption(this.Output),
                LogPath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
