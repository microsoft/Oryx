// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    public class BuildScriptGeneratorOptionsSetup : IConfigureOptions<BuildScriptGeneratorOptions>
    {
        private readonly IEnvironment _environment;

        public BuildScriptGeneratorOptionsSetup(IEnvironment environment)
        {
            _environment = environment;
        }

        public void Configure(BuildScriptGeneratorOptions options)
        {
            var enableDynamicInstall = _environment.GetBoolEnvironmentVariable(SettingsKeys.EnableDynamicInstall);
            options.EnableDynamicInstall = enableDynamicInstall.HasValue ? enableDynamicInstall.Value : false;
        }
    }
}
