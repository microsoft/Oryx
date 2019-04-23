// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal static class PythonConstants
    {
        internal const string PythonName = "python";
        internal const string RequirementsFileName = "requirements.txt";
        internal const string RuntimeFileName = "runtime.txt";
        internal const string PythonFileNamePattern = "*.py";
        internal const string PythonDefaultVersionEnvVarName = "ORYX_PYTHON_DEFAULT_VERSION";
        internal const string PythonSupportedVersionsEnvVarName = "PYTHON_SUPPORTED_VERSIONS";
        internal const string PythonLtsVersion = Common.PythonVersions.Python37Version;
        internal const string InstalledPythonVersionsDir = "/opt/python/";
        internal const string ZipFileExtension = "tar.gz";
        internal const string ZipVirtualEnvFileNameFormat = "{0}.zip";
        internal const string TarGzVirtualEnvFileNameFormat = "{0}.tar.gz";
        internal const string CompressedVirtualEnvFileBuildProperty = "compressedVirtualEnvFile";
        internal const string VirtualEnvNameBuildProperty = "virtualEnvName";
    }
}
