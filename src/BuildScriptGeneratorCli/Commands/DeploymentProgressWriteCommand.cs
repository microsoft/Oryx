// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.BuildScriptGenerator.DeploymentProgress;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class DeploymentProgressWriteCommand
    {
        internal static int OnExecute()
        {
            return DeploymentProgressReporter.AppendRecordFromStandardInput(
                Console.OpenStandardInput(),
                Environment.GetEnvironmentVariable(
                    DeploymentProgressConstants.EndpointEnvironmentVariable));
        }
    }
}
