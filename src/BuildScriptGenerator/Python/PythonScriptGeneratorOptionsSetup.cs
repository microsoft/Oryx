// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonScriptGeneratorOptionsSetup : IConfigureOptions<PythonScriptGeneratorOptions>
    {
        internal const string PythonDefaultVersion = "ORYX_PYTHON_DEFAULT_VERSION";

        // Providing the supported versions through an environment variable allows us to use the tool in
        // other environments, e.g. our local machines for debugging.
        internal const string PythonSupportedVersionsEnvVariable = "PYTHON_SUPPORTED_VERSIONS";

        internal const string PythonLtsVersion = "3.7.2";
        internal const string InstalledPythonVersionsDir = "/opt/python/";

        private readonly IEnvironment _environment;

        public PythonScriptGeneratorOptionsSetup(IEnvironment environment)
        {
            _environment = environment;
        }

        public void Configure(PythonScriptGeneratorOptions options)
        {
            var defaultVersion = _environment.GetEnvironmentVariable(PythonDefaultVersion);
            if (string.IsNullOrEmpty(defaultVersion))
            {
                defaultVersion = PythonLtsVersion;
            }

            options.PythonDefaultVersion = defaultVersion;
            options.InstalledPythonVersionsDir = InstalledPythonVersionsDir;
            options.SupportedPythonVersions = _environment.GetEnvironmentVariableAsList(PythonSupportedVersionsEnvVariable);
        }
    }
}