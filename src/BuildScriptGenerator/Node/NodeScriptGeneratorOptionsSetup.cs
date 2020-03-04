// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodeScriptGeneratorOptionsSetup : IConfigureOptions<NodeScriptGeneratorOptions>
    {
        private readonly IEnvironment _environment;

        public NodeScriptGeneratorOptionsSetup(IEnvironment environment)
        {
            _environment = environment;
        }

        public void Configure(NodeScriptGeneratorOptions options)
        {
            options.NodeVersion = _environment.GetEnvironmentVariable(NodeConstants.NodeVersion);
        }
    }
}