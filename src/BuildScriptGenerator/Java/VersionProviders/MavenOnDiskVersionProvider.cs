// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Java
{
    public class MavenOnDiskVersionProvider : IMavenVersionProvider
    {
        private const string DefaultOnDiskVersion = JavaVersions.MavenVersion;
        private readonly ILogger<MavenOnDiskVersionProvider> _logger;

        public MavenOnDiskVersionProvider(ILogger<MavenOnDiskVersionProvider> logger)
        {
            _logger = logger;
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            _logger.LogDebug(
                "Getting list of versions from {installDir}",
                JavaConstants.InstalledMavenVersionsDir);

            var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                            JavaConstants.InstalledMavenVersionsDir);

            return PlatformVersionInfo.CreateOnDiskVersionInfo(installedVersions, DefaultOnDiskVersion);
        }
    }
}
