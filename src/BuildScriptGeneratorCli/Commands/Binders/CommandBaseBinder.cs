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
    public abstract class CommandBaseBinder<T> : BinderBase<T>
        where T : CommandBaseProperty
    {
#pragma warning disable SA1401 // Fields should be private
        protected Option<string> logPath;

        protected Option<bool> debugMode;
#pragma warning restore SA1401 // Fields should be private

        public CommandBaseBinder(Option<string> logPath, Option<bool> debugMode)
        {
            this.logPath = logPath;
            this.debugMode = debugMode;
        }
    }
}
