// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using Microsoft.Extensions.Options;
using SemVer;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeVersionResolver : INodeVersionResolver
    {
        private readonly NodeScriptGeneratorOptions _options;

        public NodeVersionResolver(IOptions<NodeScriptGeneratorOptions> _nodeScriptGeneratorOptions)
        {
            _options = _nodeScriptGeneratorOptions.Value;
        }

        /// <summary>
        /// <see cref="INodeVersionProvider.GetSupportedNodeVersion(string)"/>
        /// </summary>
        public string GetSupportedNodeVersion(string versionRange)
        {
            try
            {
                var range = new Range(versionRange);
                var satisfying = range.MaxSatisfying(_options.SupportedNodeVersions);
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
                var satisfying = range.MaxSatisfying(_options.SupportedNpmVersions);
                return satisfying;
            }
            catch
            {
                return null;
            }
        }
    }
}
