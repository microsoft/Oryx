// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Ruby
{
    public class RubyOnDiskVersionProvider : IRubyVersionProvider
    {
        private readonly ILogger<RubyOnDiskVersionProvider> logger;

        public RubyOnDiskVersionProvider(ILogger<RubyOnDiskVersionProvider> logger)
        {
            this.logger = logger;
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            this.logger.LogDebug("Getting list of versions from {installDir}", RubyConstants.InstalledRubyVersionsDir);

            var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                            RubyConstants.InstalledRubyVersionsDir);

            return PlatformVersionInfo.CreateOnDiskVersionInfo(installedVersions, RubyConstants.RubyLtsVersion);
        }
    }
}
