// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;
using Microsoft.Extensions.Logging;
using SemVer;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeVersionResolver : INodeVersionResolver
    {
        private readonly INodeVersionProvider _nodeVersionProvider;
        private readonly ILogger<NodeVersionResolver> _logger;

        public NodeVersionResolver(INodeVersionProvider nodeVersionProvider, ILogger<NodeVersionResolver> logger)
        {
            _nodeVersionProvider = nodeVersionProvider;
            _logger = logger;
        }

        /// <summary>
        /// <see cref="INodeVersionProvider.GetSupportedNodeVersion(string)"/>
        /// </summary>
        public string GetSupportedNodeVersion(string versionRange)
        {
            try
            {
                var range = new Range(versionRange);
                var satisfying = range.MaxSatisfying(_nodeVersionProvider.SupportedNodeVersions);
                return satisfying;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "An error occurred while trying to find supported Node version.");
            }

            return null;
        }

        /// <summary>
        /// <see cref="INodeVersionProvider.GetSupportedNpmVersion(string)"/>
        /// </summary>
        public string GetSupportedNpmVersion(string versionRange)
        {
            try
            {
                var range = new Range(versionRange);
                var satisfying = range.MaxSatisfying(_nodeVersionProvider.SupportedNpmVersions);
                return satisfying;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "An error occurred while trying to find supported Npm version.");
            }

            return null;
        }
    }
}
