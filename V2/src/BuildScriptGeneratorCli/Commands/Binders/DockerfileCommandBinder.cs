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
            this.SourceDir = sourceDir;
            this.BuildImage = buildImage;
            this.Platform = platform;
            this.PlatformVersion = platformVersion;
            this.RuntimePlatform = runtimePlatform;
            this.RuntimePlatformVersion = runtimePlatformVersion;
            this.BindPort = bindPort;
            this.Output = output;
        }

        private Argument<string> SourceDir { get; set; }

        private Option<string> BuildImage { get; set; }

        private Option<string> Platform { get; set; }

        private Option<string> PlatformVersion { get; set; }

        private Option<string> RuntimePlatform { get; set; }

        private Option<string> RuntimePlatformVersion { get; set; }

        private Option<string> BindPort { get; set; }

        private Option<string> Output { get; set; }

        protected override DockerfileCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new DockerfileCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForArgument(this.SourceDir),
                BuildImage = bindingContext.ParseResult.GetValueForOption(this.BuildImage),
                Platform = bindingContext.ParseResult.GetValueForOption(this.Platform),
                PlatformVersion = bindingContext.ParseResult.GetValueForOption(this.PlatformVersion),
                RuntimePlatform = bindingContext.ParseResult.GetValueForOption(this.RuntimePlatform),
                RuntimePlatformVersion = bindingContext.ParseResult.GetValueForOption(this.RuntimePlatformVersion),
                BindPort = bindingContext.ParseResult.GetValueForOption(this.BindPort),
                Output = bindingContext.ParseResult.GetValueForOption(this.Output),
                LogPath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
