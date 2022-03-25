// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public class NodeOnDiskVersionProvider : INodeVersionProvider
    {
        private readonly ILogger<NodeOnDiskVersionProvider> logger;

        public NodeOnDiskVersionProvider(ILogger<NodeOnDiskVersionProvider> logger)
        {
            this.logger = logger;
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            this.logger.LogDebug("Getting list of versions from {installDir}", NodeConstants.InstalledNodeVersionsDir);

            var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                        NodeConstants.InstalledNodeVersionsDir);

            return PlatformVersionInfo.CreateOnDiskVersionInfo(
                installedVersions,
                NodeConstants.NodeLtsVersion);
        }
    }
}
