// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public class NodeOnDiskVersionProvider : INodeVersionProvider
    {
        private readonly NodeScriptGeneratorOptions _options;
        private PlatformVersionInfo _platformVersionInfo;

        public NodeOnDiskVersionProvider(IOptions<NodeScriptGeneratorOptions> options)
        {
            _options = options.Value;
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            if (_platformVersionInfo == null)
            {
                var installedVersions = VersionProviderHelper.GetVersionsFromDirectory(
                            NodeConstants.InstalledNodeVersionsDir);
                _platformVersionInfo = PlatformVersionInfo.CreateOnDiskVersionInfo(
                    installedVersions,
                    _options.NodeJsDefaultVersion);
            }

            return _platformVersionInfo;
        }
    }
}
