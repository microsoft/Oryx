// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    /// <summary>
    /// Build script template for Python in Bash.
    /// </summary>
    public class PythonBashBuildSnippetProperties
    {
        public PythonBashBuildSnippetProperties(
            string virtualEnvironmentName,
            string virtualEnvironmentModule,
            string virtualEnvironmentParameters,
            string packagesDirectory,
            bool enableCollectStatic,
            string compressVirtualEnvCommand,
            string compressedVirtualEnvFileName)
        {
            VirtualEnvironmentName = virtualEnvironmentName;
            VirtualEnvironmentModule = virtualEnvironmentModule;
            VirtualEnvironmentParameters = virtualEnvironmentParameters;
            PackagesDirectory = packagesDirectory;
            EnableCollectStatic = enableCollectStatic;
            CompressVirtualEnvCommand = compressVirtualEnvCommand;
            CompressedVirtualEnvFileName = compressedVirtualEnvFileName;
        }

        public string VirtualEnvironmentName { get; set; }

        public string VirtualEnvironmentModule { get; set; }

        public string VirtualEnvironmentParameters { get; set; }

        /// <summary>
        /// Gets or sets the directory where the packages will be downloaded to.
        /// </summary>
        public string PackagesDirectory { get; set; }

        public bool EnableCollectStatic { get; set; }

        public string CompressVirtualEnvCommand { get; set; }

        public string CompressedVirtualEnvFileName { get; set; }
    }
}