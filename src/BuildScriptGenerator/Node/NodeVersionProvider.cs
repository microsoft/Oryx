// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeVersionProvider : INodeVersionProvider
    {
        public static readonly string[] DefaultSupportedNodeVersions = new[] { ">=4 <=12" };
        public static readonly string[] DefaultSupportedNpmVersions = new[] { "<7" };

        private readonly NodeScriptGeneratorOptions _options;
        private IEnumerable<string> _supportedNodeVersions;
        private IEnumerable<string> _supportedNpmVersions;

        public NodeVersionProvider(IOptions<NodeScriptGeneratorOptions> options)
        {
            _options = options.Value;
        }

        public IEnumerable<string> SupportedNodeVersions
        {
            get
            {
                if (_supportedNodeVersions == null)
                {
                    _supportedNodeVersions = _options.SupportedNodeVersions != null
                        ? _options.SupportedNodeVersions : DefaultSupportedNodeVersions;
                }

                return _supportedNodeVersions;
            }
        }

        public IEnumerable<string> SupportedNpmVersions
        {
            get
            {
                if (_supportedNpmVersions == null)
                {
                    _supportedNpmVersions = _options.SupportedNpmVersions != null
                        ? _options.SupportedNpmVersions : DefaultSupportedNpmVersions;
                }

                return _supportedNpmVersions;
            }
        }
    }
}