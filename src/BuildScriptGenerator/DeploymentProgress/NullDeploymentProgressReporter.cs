// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DeploymentProgress
{
    internal sealed class NullDeploymentProgressReporter : IDeploymentProgressReporter
    {
        public static readonly NullDeploymentProgressReporter Instance = new NullDeploymentProgressReporter();

        private NullDeploymentProgressReporter()
        {
        }

        public bool IsEnabled => false;

        public void ReportBuildStarted()
        {
        }

        public void ReportPhaseStarted(string phase)
        {
        }

        public void ReportBuildCompleted(string outcome)
        {
        }
    }
}
