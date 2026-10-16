// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DeploymentProgress
{
    /// <summary>
    /// Reports an ordered, best-effort build lifecycle to a caller-owned local regular file.
    /// Reporting failures and missing terminal records never determine the build result.
    /// </summary>
    public interface IDeploymentProgressReporter
    {
        bool IsEnabled { get; }

        void ReportBuildStarted();

        void ReportPhaseStarted(string phase);

        void ReportBuildCompleted(string outcome);
    }
}
