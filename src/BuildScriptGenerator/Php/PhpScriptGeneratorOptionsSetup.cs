// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpScriptGeneratorOptionsSetup : IConfigureOptions<PhpScriptGeneratorOptions>
    {
        private readonly IEnvironment _env;

        public PhpScriptGeneratorOptionsSetup(IEnvironment environment)
        {
            _env = environment;
        }

        public void Configure(PhpScriptGeneratorOptions options)
        {
            var defaultVersion = _env.GetEnvironmentVariable(PhpConstants.DefaultPhpRuntimeVersionEnvVarName);
            if (string.IsNullOrWhiteSpace(defaultVersion))
            {
                defaultVersion = PhpConstants.DefaultPhpRuntimeVersion;
            }

            options.PhpDefaultVersion = defaultVersion;
            options.InstalledPhpVersionsDir = PhpConstants.InstalledPhpVersionsDir;
            options.SupportedPhpVersions = _env.GetEnvironmentVariableAsList(PhpConstants.SupportedVersionsEnvVarName);
        }
    }
}