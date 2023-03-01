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
    public class DockerfileCommandBinder : CommandBaseBinder<DockerfileCommandProperty>
    {
        private Argument<string> sourceDir;
        private Option<string> buildImage;
        private Option<string> platform;
        private Option<string> platformVersion;
        private Option<string> runtimePlatform;
        private Option<string> runtimePlatformVersion;
        private Option<string> bindPort;
        private Option<string> output;

        public DockerfileCommandBinder(
            Argument<string> sourceDir,
            Option<string> buildImage,
            Option<string> platform,
            Option<string> platformVersion,
            Option<string> runtimePlatform,
            Option<string> runtimePlatformVersion,
            Option<string> bindPort,
            Option<string> output,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.sourceDir = sourceDir;
            this.buildImage = buildImage;
            this.platform = platform;
            this.platformVersion = platformVersion;
            this.runtimePlatform = runtimePlatform;
            this.runtimePlatformVersion = runtimePlatformVersion;
            this.bindPort = bindPort;
            this.output = output;
        }

        protected override DockerfileCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new DockerfileCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForArgument(this.sourceDir),
                BuildImage = bindingContext.ParseResult.GetValueForOption(this.buildImage),
                PlatformName = bindingContext.ParseResult.GetValueForOption(this.platform),
                PlatformVersion = bindingContext.ParseResult.GetValueForOption(this.platformVersion),
                RuntimePlatformName = bindingContext.ParseResult.GetValueForOption(this.runtimePlatform),
                RuntimePlatformVersion = bindingContext.ParseResult.GetValueForOption(this.runtimePlatformVersion),
                BindPort = bindingContext.ParseResult.GetValueForOption(this.bindPort),
                OutputPath = bindingContext.ParseResult.GetValueForOption(this.output),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
