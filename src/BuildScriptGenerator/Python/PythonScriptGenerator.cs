// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Resources;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    [BuildProperty(VirtualEnvironmentNamePropertyKey, "If provided, will create a virtual environment with the given name.")]
    [BuildProperty(TargetPackageDirectoryPropertyKey, "Directory to download the packages to, if no virtual environment is provided. Default: '" + DefaultTargetPackageDirectory + "'")]
    internal class PythonScriptGenerator : ILanguageScriptGenerator
    {
        internal const string VirtualEnvironmentNamePropertyKey = "virtualenv_name";
        internal const string TargetPackageDirectoryPropertyKey = "packagedir";

        private const string PythonName = "python";
        private const string DefaultTargetPackageDirectory = "__oryx_packages__";

        private readonly PythonScriptGeneratorOptions _pythonScriptGeneratorOptions;
        private readonly IPythonVersionProvider _pythonVersionProvider;
        private readonly IEnvironmentSettingsProvider _environmentSettingsProvider;
        private readonly ILogger<PythonScriptGenerator> _logger;

        public PythonScriptGenerator(
            IOptions<PythonScriptGeneratorOptions> pythonScriptGeneratorOptions,
            IPythonVersionProvider pythonVersionProvider,
            IEnvironmentSettingsProvider environmentSettingsProvider,
            ILogger<PythonScriptGenerator> logger)
        {
            _pythonScriptGeneratorOptions = pythonScriptGeneratorOptions.Value;
            _pythonVersionProvider = pythonVersionProvider;
            _environmentSettingsProvider = environmentSettingsProvider;
            _logger = logger;
        }

        public string SupportedLanguageName => PythonName;

        public IEnumerable<string> SupportedLanguageVersions => _pythonVersionProvider.SupportedPythonVersions;

        public bool TryGenerateBashScript(ScriptGeneratorContext context, out string script)
        {
            if (context.Properties == null ||
                !context.Properties.TryGetValue(VirtualEnvironmentNamePropertyKey, out var virtualEnvName))
            {
                virtualEnvName = string.Empty;
            }

            string packageDir = null;
            if ((context.Properties == null ||
                !context.Properties.TryGetValue(TargetPackageDirectoryPropertyKey, out packageDir)) &&
                string.IsNullOrEmpty(virtualEnvName))
            {
                // Only default if no virtual environment has been provided.
                packageDir = DefaultTargetPackageDirectory;
            }

            if (!string.IsNullOrWhiteSpace(virtualEnvName) && !string.IsNullOrWhiteSpace(packageDir))
            {
                throw new InvalidUsageException(Labels.PythonBuildCantHaveVirtualEnvAndTargetPackageDirErrorMessage);
            }

            var virtualEnvModule = string.Empty;
            var virtualEnvCopyParam = string.Empty;

            var pythonVersion = context.LanguageVersion;
            _logger.LogDebug("Selected Python version: {pyVer}", pythonVersion);

            if (!string.IsNullOrEmpty(pythonVersion) && !string.IsNullOrWhiteSpace(virtualEnvName))
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
                        string errorMessage = "Python version '" + pythonVersion + "' is not supported";
                        _logger.LogError(errorMessage);
                        throw new NotSupportedException(errorMessage);
                }

                _logger.LogDebug("Using virtual environment {venv}, module {venvModule}", virtualEnvName, virtualEnvModule);
            }

            _logger.LogDependencies("Python", pythonVersion, context.SourceRepo.ReadAllLines(PythonConstants.RequirementsFileName).Where(line => !line.TrimStart().StartsWith("#")));

            _environmentSettingsProvider.TryGetAndLoadSettings(out var environmentSettings);

            script = new PythonBashBuildScript(
                preBuildScriptPath: environmentSettings?.PreBuildScriptPath,
                benvArgs: $"python={pythonVersion}",
                virtualEnvironmentName: virtualEnvName,
                virtualEnvironmentModule: virtualEnvModule,
                virtualEnvironmentParameters: virtualEnvCopyParam,
                packagesDirectory: packageDir,
                postBuildScriptPath: environmentSettings?.PostBuildScriptPath).TransformText();

            return true;
        }
    }
}