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
    public class BuildScriptCommandBinder : BuildCommandBaseBinder<BuildScriptCommandProperty>
    {
        private Option<string> buildScriptOutput;

        public BuildScriptCommandBinder(
            Option<string> buildScriptOutput,
            Argument<string> sourceDir,
            Option<string> platform,
            Option<string> platformVersion,
            Option<bool> package,
            Option<string> osRequirements,
            Option<string> appType,
            Option<string> buildCommandFile,
            Option<bool> compressDestinationDir,
            Option<string[]> property,
            Option<string> dynamicInstallRootDir,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(sourceDir, platform, platformVersion, package, osRequirements, appType, buildCommandFile, compressDestinationDir, property, dynamicInstallRootDir, logPath, debugMode)
        {
            this.buildScriptOutput = buildScriptOutput;
        }

        protected override BuildScriptCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new BuildScriptCommandProperty
            {
                OutputPath = bindingContext.ParseResult.GetValueForOption(this.buildScriptOutput),
                SourceDir = bindingContext.ParseResult.GetValueForArgument(this.SourceDir),
                PlatformName = bindingContext.ParseResult.GetValueForOption(this.Platform),
                PlatformVersion = bindingContext.ParseResult.GetValueForOption(this.PlatformVersion),
                ShouldPackage = bindingContext.ParseResult.GetValueForOption(this.Package),
                OsRequirements = bindingContext.ParseResult.GetValueForOption(this.OsRequirements),
                AppType = bindingContext.ParseResult.GetValueForOption(this.AppType),
                BuildCommandsFileName = bindingContext.ParseResult.GetValueForOption(this.BuildCommandFile),
                CompressDestinationDir = bindingContext.ParseResult.GetValueForOption(this.CompressDestinationDir),
                Properties = bindingContext.ParseResult.GetValueForOption(this.Property),
                DynamicInstallRootDir = bindingContext.ParseResult.GetValueForOption(this.DynamicInstallRootDir),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
