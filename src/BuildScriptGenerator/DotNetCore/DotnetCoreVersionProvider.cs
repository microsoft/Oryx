// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotNetCoreVersionProvider : IDotNetCoreVersionProvider
    {
        private PlatformVersionInfo _versionInfo;

        public PlatformVersionInfo GetVersionInfo()
        {
            if (_versionInfo == null)
            {
                var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                            DotNetCoreConstants.InstalledRuntimeVersionsDir);
                _versionInfo = PlatformVersionInfo.CreateOnDiskVersionInfo(
                    installedVersions,
                    DotNetCoreConstants.RuntimeLtsVersion);
            }

            return _versionInfo;
        }
    }
}