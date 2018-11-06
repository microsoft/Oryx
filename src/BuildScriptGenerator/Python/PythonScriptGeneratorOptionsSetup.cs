// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonScriptGeneratorOptionsSetup : IConfigureOptions<PythonScriptGeneratorOptions>
    {
        internal const string PythonDefaultVersion = "ORYX_PYTHON_DEFAULT_VERSION";
        internal const string PythonLtsVersion = "3.7.1";
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
        }
    }
}
