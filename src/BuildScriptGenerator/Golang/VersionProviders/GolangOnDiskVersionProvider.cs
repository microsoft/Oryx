// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Golang
{
    public class GolangOnDiskVersionProvider : IGolangVersionProvider
    {
        private readonly ILogger<GolangOnDiskVersionProvider> _logger;

        public GolangOnDiskVersionProvider(ILogger<GolangOnDiskVersionProvider> logger)
        {
            _logger = logger;
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            _logger.LogDebug("Getting list of versions from {installDir}", GolangConstants.InstalledGolangVersionsDir);

            var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                            GolangConstants.InstalledGolangVersionsDir);

            return PlatformVersionInfo.CreateOnDiskVersionInfo(installedVersions, GolangConstants.GolangDefaultVersion);
        }
    }
}
