// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Oryx.Tests.Common
{
    public abstract class DockerResultBase
    {
        public DockerResultBase(Exception exc, string executedCmd)
        {
            Exception = exc;
            ExecutedCommand = executedCmd;
        }

        public Exception Exception { get; }

        public string ExecutedCommand { get; }

        public abstract bool HasExited { get; }

        public abstract int ExitCode { get; }

        public abstract string StdOut { get; }

        public abstract string StdErr { get; }

        public virtual string GetDebugInfo(IDictionary<string, string> extraDefs = null)
        {
            var sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine("Debugging Information:");
            sb.AppendLine("----------------------");

            var infoFormatter = new DefinitionListFormatter();

            // NOTE: do not log the executed command as it might contain secrets
            //infoFormatter.AddDefinition("Executed command", ExecutedCommand);
            if (HasExited) infoFormatter.AddDefinition("Exit code", ExitCode.ToString());
            infoFormatter.AddDefinition("StdOut", StdOut);
            infoFormatter.AddDefinition("StdErr", StdErr);
            infoFormatter.AddDefinition("Exception.Message:", Exception?.Message);
            infoFormatter.AddDefinitions(extraDefs);

            sb.AppendLine(infoFormatter.ToString());

            return sb.ToString();
        }
    }
}
