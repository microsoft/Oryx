// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public class NodeOnDiskVersionProvider : INodeVersionProvider
    {
        private readonly ILogger<NodeOnDiskVersionProvider> logger;
        private readonly BuildScriptGeneratorOptions options;

        public NodeOnDiskVersionProvider(
            IOptions<BuildScriptGeneratorOptions> options,
            ILogger<NodeOnDiskVersionProvider> logger)
        {
            this.logger = logger;
            this.options = options.Value;
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            this.logger.LogDebug("Getting list of versions from {installDir}", NodeConstants.InstalledNodeVersionsDir);

            var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                        NodeConstants.InstalledNodeVersionsDir);

            return PlatformVersionInfo.CreateOnDiskVersionInfo(
                installedVersions,
                this.options.DebianFlavor != OsTypes.DebianStretch
                    ? NodeConstants.NodeLtsVersion
                    : FinalStretchVersions.FinalStretchNode14Version);
        }
    }
}
