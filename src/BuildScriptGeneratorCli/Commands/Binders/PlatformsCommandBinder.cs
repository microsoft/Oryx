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
        private Option<bool> jsonOption;

        public PlatformsCommandBinder(
            Option<bool> jsonOption,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.jsonOption = jsonOption;
        }

        protected override PlatformsCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new PlatformsCommandProperty
            {
                OutputJson = bindingContext.ParseResult.GetValueForOption(this.jsonOption),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
