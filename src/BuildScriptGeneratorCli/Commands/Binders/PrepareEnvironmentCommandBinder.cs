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
        public PrepareEnvironmentCommandBinder(
            Option<string> sourceDir,
            Option<bool> skipDetection,
            Option<string> platformsAndVersions,
            Option<string> platformsAndVersionsFile,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.SourceDir = sourceDir;
            this.SkipDetection = skipDetection;
            this.PlatformsAndVersions = platformsAndVersions;
            this.PlatformsAndVersionsFile = platformsAndVersionsFile;
        }

        private Option<string> SourceDir { get; set; }

        private Option<bool> SkipDetection { get; set; }

        private Option<string> PlatformsAndVersions { get; set; }

        private Option<string> PlatformsAndVersionsFile { get; set; }

        protected override PrepareEnvironmentCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new PrepareEnvironmentCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForOption(this.SourceDir),
                SkipDetection = bindingContext.ParseResult.GetValueForOption(this.SkipDetection),
                PlatformsAndVersions = bindingContext.ParseResult.GetValueForOption(this.PlatformsAndVersions),
                PlatformsAndVersionsFile = bindingContext.ParseResult.GetValueForOption(this.PlatformsAndVersionsFile),
                LogPath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
