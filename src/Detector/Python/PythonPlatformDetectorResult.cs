// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector.Python
{
    /// <summary>
    /// Represents the model which contains Python specific detected metadata.
    /// </summary>
    public class PythonPlatformDetectorResult : PlatformDetectorResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether any files of extension '.ipynb' exist at the root of the repo.
        /// </summary>
        public bool HasJupyterNotebookFiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'environment.yml' or 'environment.yaml' files
        /// exist at the root of the repo.
        /// </summary>
        public bool HasCondaEnvironmentYmlFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a 'requirements.txt' file exists at the root of the repo.
        /// </summary>
        public bool HasRequirementsTxtFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a 'pyproject.toml' file exists at the root of the repo.
        /// </summary>
        public bool HasPyprojectTomlFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a 'uv.lock' file exists at the root of the repo.
        /// </summary>
        public bool HasUvLockFile { get; set; }
    }
}
