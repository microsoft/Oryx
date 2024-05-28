// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Golang
{
    public class GolangOnDiskVersionProvider : IGolangVersionProvider
    {
        private readonly ILogger<GolangOnDiskVersionProvider> logger;

        public GolangOnDiskVersionProvider(ILogger<GolangOnDiskVersionProvider> logger)
        {
            this.logger = logger;
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            // we expect Golang to only be installed using dynamic installation
            this.logger.LogDebug("Getting list of versions from {installDir}", GolangConstants.DynamicInstalledGolangVersionsDir);

            var installedVersions = VersionProviderHelper.GetMajorMinorVersionsFromDirectory(
                            GolangConstants.DynamicInstalledGolangVersionsDir);

            return PlatformVersionInfo.CreateOnDiskVersionInfo(installedVersions, GolangConstants.GolangDefaultVersion);
        }
    }
}
