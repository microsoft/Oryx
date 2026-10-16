// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DeploymentProgress
{
    internal interface IDeploymentProgressWriterProcessFactory
    {
        IDeploymentProgressWriterProcess Start(string endpointPath);
    }
}
