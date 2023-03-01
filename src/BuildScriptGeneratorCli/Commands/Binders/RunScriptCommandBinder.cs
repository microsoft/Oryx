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
        private Argument<string> appDir;
        private Option<string> platformName;
        private Option<string> platformVersion;
        private Option<string> output;

        public RunScriptCommandBinder(
            Argument<string> appDir,
            Option<string> platformName,
            Option<string> platformVersion,
            Option<string> output,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.appDir = appDir;
            this.platformName = platformName;
            this.platformVersion = platformVersion;
            this.output = output;
        }

        protected override RunScriptCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new RunScriptCommandProperty
            {
                AppDir = bindingContext.ParseResult.GetValueForArgument(this.appDir),
                PlatformName = bindingContext.ParseResult.GetValueForOption(this.platformName),
                PlatformVersion = bindingContext.ParseResult.GetValueForOption(this.platformVersion),
                OutputPath = bindingContext.ParseResult.GetValueForOption(this.output),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
