// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Node;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    public class NodeScriptGeneratorOptionsSetup : IConfigureOptions<NodeScriptGeneratorOptions>
    {
        private readonly IConfiguration _config;

        public NodeScriptGeneratorOptionsSetup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public void Configure(NodeScriptGeneratorOptions options)
        {
            options.NodeVersion = _config.GetValue<string>(SettingsKeys.NodeVersion);
        }
    }
}
