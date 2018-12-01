// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    [BuildProperty(VirtualEnvironmentNamePropertyKey, "Name of the virtual environment to create.")]
    internal class PythonScriptGenerator : ILanguageScriptGenerator
    {
        internal const string VirtualEnvironmentNamePropertyKey = "virtualenv_name";

        private const string PythonName = "python";
        private const string DefaultVirtualEnvironmentName = "pythonenv";

        private readonly PythonScriptGeneratorOptions _pythonScriptGeneratorOptions;
        private readonly IPythonVersionProvider _pythonVersionProvider;
        private readonly ILogger<PythonScriptGenerator> _logger;

        public PythonScriptGenerator(
            IOptions<PythonScriptGeneratorOptions> pythonScriptGeneratorOptions,
            IPythonVersionProvider pythonVersionProvider,
            ILogger<PythonScriptGenerator> logger)
        {
            _pythonScriptGeneratorOptions = pythonScriptGeneratorOptions.Value;
            _pythonVersionProvider = pythonVersionProvider;
            _logger = logger;
        }

        public string SupportedLanguageName => PythonName;

        public IEnumerable<string> SupportedLanguageVersions => _pythonVersionProvider.SupportedPythonVersions;

        public bool TryGenerateBashScript(ScriptGeneratorContext context, out string script)
        {
            if (context.Properties == null ||
                !context.Properties.TryGetValue(VirtualEnvironmentNamePropertyKey, out var virtualEnvName))
            {
                virtualEnvName = DefaultVirtualEnvironmentName;
            }

            var virtualEnvModule = "venv";
            var virtualEnvCopyParam = string.Empty;

            var pythonVersion = context.LanguageVersion;
            if (!string.IsNullOrEmpty(pythonVersion))
            {
                switch (pythonVersion.Split('.')[0])
                {
                    case "2":
                        virtualEnvModule = "virtualenv";
                        break;

                    case "3":
                        virtualEnvModule = "venv";
                        virtualEnvCopyParam = "--copies";
                        break;

                    default:
                        string errorMessage = "Python version " + pythonVersion + " is not supported";
                        _logger.LogError(errorMessage);
                        throw new NotSupportedException(errorMessage);
                }
            }

            _logger.LogDebug("Selected Python version: {PyVer}; venv module: {VenvModule}", pythonVersion, virtualEnvModule);

            script = new PythonBashBuildScript(
                virtualEnvironmentName: virtualEnvName,
                virtualEnvironmentModule: virtualEnvModule,
                virtualEnvironmentParameters: virtualEnvCopyParam,
                pythonVersion: pythonVersion
            ).TransformText();

            return true;
        }
    }
}