// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGeneratorCli.DeploymentProgress;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class DeploymentProgressWriteCommand
    {
        internal const string Name = "deployment-progress-write";

        internal static Command Export()
        {
            var phaseOption = new Option<string>("--phase");
            var command = new Command(Name, "[INTERNAL ONLY COMMAND]")
            {
                phaseOption,
            };
            command.IsHidden = true;
            command.SetHandler(
                phase => Task.FromResult(Execute(phase)),
                phaseOption);
            return command;
        }

        internal static int Execute(string phase)
        {
            return DeploymentProgressWriter.TryAppendPhaseFromEnvironment(phase)
                ? ProcessConstants.ExitSuccess
                : ProcessConstants.ExitFailure;
        }
    }
}
