// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using System;
using System.Linq;

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
            Console.WriteLine($"Entered on-disk version provider");
            var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                            RubyConstants.InstalledRubyVersionsDir);
            Console.WriteLine($"Figure out installedVersions : {installedVersions.Count()}");
            foreach (var installedVersion in installedVersions)
            {
                Console.WriteLine($"Ruby installed version : {installedVersion}");
            }

            return PlatformVersionInfo.CreateOnDiskVersionInfo(installedVersions, RubyConstants.RubyLtsVersion);
        }
    }
}
