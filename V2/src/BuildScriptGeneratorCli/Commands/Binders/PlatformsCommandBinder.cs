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
    public class PlatformsCommandBinder : CommandBaseBinder<PlatformsCommandProperty>
    {
        public PlatformsCommandBinder(
            Option<bool> outputJson,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.OutputJson = outputJson;
        }

        private Option<bool> OutputJson { get; set; }

        protected override PlatformsCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new PlatformsCommandProperty
            {
                OutputJson = bindingContext.ParseResult.GetValueForOption(this.OutputJson),
                LogPath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
