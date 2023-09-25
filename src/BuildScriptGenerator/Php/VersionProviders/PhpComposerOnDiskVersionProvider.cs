// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    public class PhpComposerOnDiskVersionProvider : IPhpComposerVersionProvider
    {
        private const string DefaultOnDiskVersion = PhpVersions.ComposerDefaultVersion;
        private readonly ILogger<PhpComposerOnDiskVersionProvider> logger;

        public PhpComposerOnDiskVersionProvider(ILogger<PhpComposerOnDiskVersionProvider> logger)
        {
            this.logger = logger;
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            this.logger.LogDebug(
                "Getting list of versions from {installDir}",
                PhpConstants.InstalledPhpComposerVersionDir);

            var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                            PhpConstants.InstalledPhpComposerVersionDir);

            return PlatformVersionInfo.CreateOnDiskVersionInfo(installedVersions, DefaultOnDiskVersion);
        }
    }
}
