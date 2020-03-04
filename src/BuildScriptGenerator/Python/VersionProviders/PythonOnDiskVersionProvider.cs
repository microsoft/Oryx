// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public class PythonOnDiskVersionProvider : IPythonVersionProvider
    {
        private const string DefaultOnDiskVersion = PythonConstants.PythonLtsVersion;

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                            PythonConstants.InstalledPythonVersionsDir);

            return PlatformVersionInfo.CreateOnDiskVersionInfo(installedVersions, DefaultOnDiskVersion);
        }
    }
}
