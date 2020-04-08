// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public class NodeOnDiskVersionProvider : INodeVersionProvider
    {
        private readonly ILogger<NodeOnDiskVersionProvider> _logger;

        public NodeOnDiskVersionProvider(ILogger<NodeOnDiskVersionProvider> logger)
        {
            _logger = logger;
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            _logger.LogDebug("Getting list of versions from {installDir}", NodeConstants.InstalledNodeVersionsDir);

            var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                        NodeConstants.InstalledNodeVersionsDir);

            return PlatformVersionInfo.CreateOnDiskVersionInfo(
                installedVersions,
                NodeConstants.NodeLtsVersion);
        }
    }
}
