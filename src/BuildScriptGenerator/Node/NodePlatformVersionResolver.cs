// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using System.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodePlatformVersionResolver : IPlatformVersionResolver
    {
        private readonly ILogger<NodePlatformVersionResolver> _logger;
        private readonly INodeVersionProvider _versionProvider;

        public NodePlatformVersionResolver(
           ILogger<NodePlatformVersionResolver> logger,
           INodeVersionProvider versionProvider)
        {
            _logger = logger;
            _versionProvider = versionProvider;
        }


        public string GetMaxSatisfyingVersionAndVerify(string version)
        {
            var versionInfo = _versionProvider.GetVersionInfo();
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                version,
                versionInfo.SupportedVersions);

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exception = new UnsupportedVersionException(
                    NodeConstants.PlatformName,
                    version,
                    versionInfo.SupportedVersions);
                _logger.LogError(
                    exception,
                    $"Exception caught, the version '{version}' is not supported for the Node platform.");
                throw exception;
            }

            return maxSatisfyingVersion;
        }

        public string GetDefaultVersionFromProvider()
        {
            var versionInfo = _versionProvider.GetVersionInfo();
            return versionInfo.DefaultVersion;
        }
    }
}
