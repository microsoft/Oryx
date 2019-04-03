// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

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
            bool disableCollectStatic,
            bool zipVirtualEnvDir,
            IEnumerable<string> directoriesToExcludeFromCopyToBuildOutputDir,
            bool zipAllOutput)
        {
            VirtualEnvironmentName = virtualEnvironmentName;
            VirtualEnvironmentModule = virtualEnvironmentModule;
            VirtualEnvironmentParameters = virtualEnvironmentParameters;
            PackagesDirectory = packagesDirectory;
            DisableCollectStatic = disableCollectStatic;
            ZipVirtualEnvDir = zipVirtualEnvDir;
            DirectoriesToExcludeFromCopyToBuildOutputDir = directoriesToExcludeFromCopyToBuildOutputDir;
            ZipAllOutput = zipAllOutput;
        }

        public string VirtualEnvironmentName { get; set; }

        public string VirtualEnvironmentModule { get; set; }

        public string VirtualEnvironmentParameters { get; set; }

        /// <summary>
        /// Gets or sets the directory where the packages will be downloaded to.
        /// </summary>
        public string PackagesDirectory { get; set; }

        public bool DisableCollectStatic { get; set; }

        public bool ZipVirtualEnvDir { get; set; }

        public IEnumerable<string> DirectoriesToExcludeFromCopyToBuildOutputDir { get; set; }

        public bool ZipAllOutput { get; set; }
    }
}