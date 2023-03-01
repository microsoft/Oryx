// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.CommandLine;
using System.CommandLine.Binding;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class PrepareEnvironmentCommandBinder : CommandBaseBinder<PrepareEnvironmentCommandProperty>
    {
        private Option<string> sourceDir;
        private Option<bool> skipDetection;
        private Option<string> platformsAndVersions;
        private Option<string> platformsAndVersionsFile;

        public PrepareEnvironmentCommandBinder(
            Option<string> sourceDirOption,
            Option<bool> skipDetection,
            Option<string> platformsAndVersions,
            Option<string> platformsAndVersionsFile,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.sourceDir = sourceDirOption;
            this.skipDetection = skipDetection;
            this.platformsAndVersions = platformsAndVersions;
            this.platformsAndVersionsFile = platformsAndVersionsFile;
        }

        protected override PrepareEnvironmentCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new PrepareEnvironmentCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForOption(this.sourceDir),
                SkipDetection = bindingContext.ParseResult.GetValueForOption(this.skipDetection),
                PlatformsAndVersions = bindingContext.ParseResult.GetValueForOption(this.platformsAndVersions),
                PlatformsAndVersionsFile = bindingContext.ParseResult.GetValueForOption(this.platformsAndVersionsFile),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
