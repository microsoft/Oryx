// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Node;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    /// <summary>
    /// Gets hierarchical configuration from IConfiguration api and binds the properties on NodeScriptGeneratorOptions
    /// </summary>
    public class NodeScriptGeneratorOptionsSetup : IConfigureOptions<NodeScriptGeneratorOptions>
    {
        private readonly IEnvironment _environment;

        public NodeScriptGeneratorOptionsSetup(IEnvironment environment)
        {
            _environment = environment;
        }

        public void Configure(NodeScriptGeneratorOptions options)
        {
            options.NodeVersion = _environment.GetEnvironmentVariable(SettingsKeys.NodeVersion);
            options.CustomRunBuildCommand = _environment.GetEnvironmentVariable(SettingsKeys.CustomRunBuildCommand);
        }
    }
}
