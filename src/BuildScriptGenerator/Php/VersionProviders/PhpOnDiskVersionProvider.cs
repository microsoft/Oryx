// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    public class PhpOnDiskVersionProvider : IPhpVersionProvider
    {
        private const string DefaultOnDiskVersion = PhpConstants.DefaultPhpRuntimeVersion;
        private readonly ILogger<PhpOnDiskVersionProvider> _logger;

        public PhpOnDiskVersionProvider(ILogger<PhpOnDiskVersionProvider> logger)
        {
            _logger = logger;
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            _logger.LogDebug("Getting list of versions from {installDir}", PhpConstants.InstalledPhpVersionsDir);

            var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                            PhpConstants.InstalledPhpVersionsDir);

            return PlatformVersionInfo.CreateOnDiskVersionInfo(installedVersions, DefaultOnDiskVersion);
        }
    }
}
