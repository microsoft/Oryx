// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonScriptGeneratorOptionsSetup : IConfigureOptions<PythonScriptGeneratorOptions>
    {
        private readonly IEnvironment _environment;

        public PythonScriptGeneratorOptionsSetup(IEnvironment environment)
        {
            _environment = environment;
        }

        public void Configure(PythonScriptGeneratorOptions options)
        {
            var defaultVersion = _environment.GetEnvironmentVariable(PythonConstants.PythonDefaultVersionEnvVarName);
            if (string.IsNullOrEmpty(defaultVersion))
            {
                defaultVersion = PythonConstants.PythonLtsVersion;
            }

            options.PythonDefaultVersion = defaultVersion;
            options.InstalledPythonVersionsDir = PythonConstants.InstalledPythonVersionsDir;

            // Providing the supported versions through an environment variable allows us to use the tool in
            // other environments, e.g. our local machines for debugging.
            options.SupportedPythonVersions = _environment.GetEnvironmentVariableAsList(
                PythonConstants.PythonSupportedVersionsEnvVarName);
        }
    }
}