// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    /// <summary>
    /// Build script template for Python in Bash.
    /// </summary>
    public partial class PythonBashBuildScript
    {
        public PythonBashBuildScript(
            string virtualEnvironmentName,
            string virtualEnvironmentModule,
            string virtualEnvironmentParameters,
            string pythonVersion)
        {
            this.VirtualEnvironmentName = virtualEnvironmentName;
            this.VirtualEnvironmentModule = virtualEnvironmentModule;
            this.VirtualEnvironmentParameters = virtualEnvironmentParameters;
            this.PythonVersion = pythonVersion;
        }

        public string VirtualEnvironmentName { get; set; }

        public string VirtualEnvironmentModule { get; set; }

        public string VirtualEnvironmentParameters { get; set; }

        public string PythonVersion { get; set; }
    }
}