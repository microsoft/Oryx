// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.CommandLine;
using System.CommandLine.Binding;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public abstract class CommandBaseBinder<T> : BinderBase<T>
        where T : CommandBaseProperty
    {
        public CommandBaseBinder(Option<string> logPath, Option<bool> debugMode)
        {
            this.LogPath = logPath;
            this.DebugMode = debugMode;
        }

        protected Option<string> LogPath { get; set; }

        protected Option<bool> DebugMode { get; set; }
    }
}
