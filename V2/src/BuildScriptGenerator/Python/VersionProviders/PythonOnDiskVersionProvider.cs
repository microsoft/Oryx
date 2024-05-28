// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public class PythonOnDiskVersionProvider : IPythonVersionProvider
    {
        private const string DefaultOnDiskVersion = PythonConstants.PythonLtsVersion;
        private readonly ILogger<PythonOnDiskVersionProvider> logger;

        public PythonOnDiskVersionProvider(ILogger<PythonOnDiskVersionProvider> logger)
        {
            this.logger = logger;
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            this.logger.LogDebug("Getting list of versions from {installDir}", PythonConstants.InstalledPythonVersionsDir);

            var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                            PythonConstants.InstalledPythonVersionsDir);

            return PlatformVersionInfo.CreateOnDiskVersionInfo(installedVersions, DefaultOnDiskVersion);
        }
    }
}
