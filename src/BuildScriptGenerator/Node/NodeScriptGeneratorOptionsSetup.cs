// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeScriptGeneratorOptionsSetup : IConfigureOptions<NodeScriptGeneratorOptions>
    {
        internal const string NodeJsDefaultVersion = "NODE_DEFAULT_VERSION";
        internal const string NpmDefaultVersion = "NPM_DEFAULT_VERSION";

        private readonly IEnvironment _environment;
        private readonly INodeVersionProvider _nodeVersionProvider;

        public NodeScriptGeneratorOptionsSetup(IEnvironment environment, INodeVersionProvider nodeVersionProvider)
        {
            _environment = environment;
            _nodeVersionProvider = nodeVersionProvider;
        }

        public void Configure(NodeScriptGeneratorOptions options)
        {
            options.NodeJsDefaultVersion = _environment.GetEnvironmentVariable(NodeJsDefaultVersion);

            options.NpmDefaultVersion = _environment.GetEnvironmentVariable(NpmDefaultVersion);

            options.SupportedNodeVersions = _nodeVersionProvider.SupportedNodeVersions;

            options.SupportedNpmVersions = _nodeVersionProvider.SupportedNpmVersions;
        }
    }
}
