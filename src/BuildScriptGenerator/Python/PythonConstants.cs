// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal static class PythonConstants
    {
        internal const string PlatformName = "python";
        internal const string RequirementsFileName = "requirements.txt";
        internal const string RuntimeFileName = "runtime.txt";
        internal const string PythonFileNamePattern = "*.py";
        internal const string PythonVersionEnvVarName = "PYTHON_VERSION";
        internal const string PythonLtsVersion = Common.PythonVersions.Python38Version;
        internal const string InstalledPythonVersionsDir = "/opt/python/";
        internal const string ZipFileExtension = "tar.gz";
        internal const string ZipVirtualEnvFileNameFormat = "{0}.zip";
        internal const string TarGzVirtualEnvFileNameFormat = "{0}.tar.gz";
        internal const string DefaultTargetPackageDirectory = "__oryx_packages__";
        internal const string SetupDotPyFileName = "setup.py";
    }
}
