// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Php;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Options
{
    /// <summary>
    /// Gets hierarchical configuration from IConfiguration api and
    /// binds the properties on <see cref="PhpScriptGeneratorOptions"/>.
    /// </summary>
    public class PhpScriptGeneratorOptionsSetup : IConfigureOptions<PhpScriptGeneratorOptions>
    {
        private readonly IEnvironment _environment;

        public PhpScriptGeneratorOptionsSetup(IEnvironment environment)
        {
            _environment = environment;
        }

        public void Configure(PhpScriptGeneratorOptions options)
        {
            options.PhpVersion = _environment.GetEnvironmentVariable(SettingsKeys.PhpVersion);
        }
    }
}