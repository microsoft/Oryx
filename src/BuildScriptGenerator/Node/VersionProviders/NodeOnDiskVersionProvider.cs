// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public class NodeOnDiskVersionProvider : INodeVersionProvider
    {
        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                        NodeConstants.InstalledNodeVersionsDir);

            return PlatformVersionInfo.CreateOnDiskVersionInfo(
                installedVersions,
                NodeConstants.NodeLtsVersion);
        }
    }
}
