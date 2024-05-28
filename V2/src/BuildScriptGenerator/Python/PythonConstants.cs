// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public static class PythonConstants
    {
        public const string PlatformName = "python";
        public const string RequirementsFileName = "requirements.txt";
        public const string RuntimeFileName = "runtime.txt";
        public const string PythonFileNamePattern = "*.py";
        public const string PythonVersionEnvVarName = "PYTHON_VERSION";
        public const string PythonLtsVersion = Common.PythonVersions.Python38Version;
        public const string InstalledPythonVersionsDir = "/opt/python/";
        public const string ZipFileExtension = "tar.gz";
        public const string ZipVirtualEnvFileNameFormat = "{0}.zip";
        public const string TarGzVirtualEnvFileNameFormat = "{0}.tar.gz";
        public const string DefaultTargetPackageDirectory = "__oryx_packages__";
        public const string SetupDotPyFileName = "setup.py";
        public const string CondaExecutablePath = "/opt/conda/condabin/conda";
    }
}
