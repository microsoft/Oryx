// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Java
{
    internal class JavaOnDiskVersionProvider : IJavaVersionProvider
    {
        private readonly ILogger<JavaOnDiskVersionProvider> logger;

        public JavaOnDiskVersionProvider(ILogger<JavaOnDiskVersionProvider> logger)
        {
            this.logger = logger;
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            this.logger.LogDebug("Getting list of versions from {installDir}", JavaConstants.InstalledJavaVersionsDir);

            var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                        JavaConstants.InstalledJavaVersionsDir);

            return PlatformVersionInfo.CreateOnDiskVersionInfo(
                installedVersions,
                JavaConstants.JavaLtsVersion);
        }
    }
}
