// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Node;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    /// <summary>
    /// Gets hierarchical configuration from IConfiguration api and binds the properties on NodeScriptGeneratorOptions.
    /// </summary>
    public class NodeScriptGeneratorOptionsSetup : OptionsSetupBase, IConfigureOptions<NodeScriptGeneratorOptions>
    {
        public NodeScriptGeneratorOptionsSetup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public void Configure(NodeScriptGeneratorOptions options)
        {
            options.NodeVersion = this.GetStringValue(SettingsKeys.NodeVersion);
            options.DefaultVersion = this.GetStringValue(SettingsKeys.NodeDefaultVersion);
            options.CustomRunBuildCommand = this.GetStringValue(SettingsKeys.CustomRunBuildCommand);
            options.CustomBuildCommand = this.GetStringValue(SettingsKeys.CustomBuildCommand);
            options.PruneDevDependencies = this.GetBooleanValue(SettingsKeys.PruneDevDependencies);
            options.NpmRegistryUrl = this.GetStringValue(SettingsKeys.NpmRegistryUrl);
            options.EnableNodeMonorepoBuild = this.GetBooleanValue(SettingsKeys.EnableNodeMonorepoBuild);
            options.YarnTimeOutConfig = this.GetStringValue(SettingsKeys.YarnTimeOutConfig);
        }
    }
}
