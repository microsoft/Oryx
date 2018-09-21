// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using SemVer;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeVersionResolver : INodeVersionResolver
    {
        private readonly INodeVersionProvider _nodeVersionProvider;

        public NodeVersionResolver(INodeVersionProvider nodeVersionProvider)
        {
            _nodeVersionProvider = nodeVersionProvider;
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
            catch
            {
                return null;
            }
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
            catch
            {
                return null;
            }
        }
    }
}
