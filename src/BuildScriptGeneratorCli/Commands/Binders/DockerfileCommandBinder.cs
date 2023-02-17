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
        private Argument<string> sourceDirArgument;
        private Option<string> buildImageOption;
        private Option<string> platformOption;
        private Option<string> platformVersionOption;
        private Option<string> runtimePlatformOption;
        private Option<string> runtimePlatformVersionOption;
        private Option<string> bindPortOption;
        private Option<string> outputOption;

        public DockerfileCommandBinder(
            Argument<string> sourceDirArgument,
            Option<string> buildImageOption,
            Option<string> platformOption,
            Option<string> platformVersionOption,
            Option<string> runtimePlatformOption,
            Option<string> runtimePlatformVersionOption,
            Option<string> bindPortOption,
            Option<string> outputOption,
            Option<string> logFileOption,
            Option<bool> debugMode)
            : base(logFileOption, debugMode)
        {
            this.sourceDirArgument = sourceDirArgument;
            this.buildImageOption = buildImageOption;
            this.platformOption = platformOption;
            this.platformVersionOption = platformVersionOption;
            this.runtimePlatformOption = runtimePlatformOption;
            this.runtimePlatformVersionOption = runtimePlatformVersionOption;
            this.bindPortOption = bindPortOption;
            this.outputOption = outputOption;
        }

        protected override DockerfileCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new DockerfileCommandProperty
            {
                SourceDir = bindingContext.ParseResult.GetValueForArgument(this.sourceDirArgument),
                BuildImage = bindingContext.ParseResult.GetValueForOption(this.buildImageOption),
                PlatformName = bindingContext.ParseResult.GetValueForOption(this.platformOption),
                PlatformVersion = bindingContext.ParseResult.GetValueForOption(this.platformVersionOption),
                RuntimePlatformName = bindingContext.ParseResult.GetValueForOption(this.runtimePlatformOption),
                RuntimePlatformVersion = bindingContext.ParseResult.GetValueForOption(this.runtimePlatformVersionOption),
                BindPort = bindingContext.ParseResult.GetValueForOption(this.bindPortOption),
                OutputPath = bindingContext.ParseResult.GetValueForOption(this.outputOption),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.logPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.debugMode),
            };
    }
}
