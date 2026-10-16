// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;

namespace Microsoft.Oryx.BuildScriptGenerator.DeploymentProgress
{
    internal interface IDeploymentProgressWriterProcess : IDisposable
    {
        Stream StandardInput { get; }

        int ExitCode { get; }

        bool WaitForExit(TimeSpan timeout);

        void Kill();
    }
}
