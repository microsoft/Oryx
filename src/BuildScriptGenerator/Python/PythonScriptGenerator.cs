// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
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
        private readonly IEnvironment _environment;
        private readonly ILogger<PythonScriptGenerator> _logger;

        public PythonScriptGenerator(
            IOptions<PythonScriptGeneratorOptions> pythonScriptGeneratorOptions,
            IPythonVersionProvider pythonVersionProvider,
            IEnvironment environment,
            ILogger<PythonScriptGenerator> logger)
        {
            _pythonScriptGeneratorOptions = pythonScriptGeneratorOptions.Value;
            _pythonVersionProvider = pythonVersionProvider;
            _environment = environment;
            _logger = logger;
        }

        public string SupportedLanguageName => PythonName;

        public IEnumerable<string> SupportedLanguageVersions => _pythonVersionProvider.SupportedPythonVersions;

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(ScriptGeneratorContext context)
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

                _logger.LogDebug(
                    "Using virtual environment {venv}, module {venvModule}",
                    virtualEnvName,
                    virtualEnvModule);
            }

            // Collect static is enabled by default, but users can opt-out of it
            var disableCollectStatic = false;
            var disableCollectStaticEnvValue = _environment.GetEnvironmentVariable(
                EnvironmentSettingsKeys.DisableCollectStatic);
            if (string.Equals(disableCollectStaticEnvValue, "true", StringComparison.OrdinalIgnoreCase))
            {
                disableCollectStatic = true;
            }

            _logger.LogDependencies(
                "Python",
                pythonVersion,
                context.SourceRepo.ReadAllLines(PythonConstants.RequirementsFileName)
                .Where(line => !line.TrimStart().StartsWith("#")));

            var script = new PythonBashBuildSnippet(
                virtualEnvironmentName: virtualEnvName,
                virtualEnvironmentModule: virtualEnvModule,
                virtualEnvironmentParameters: virtualEnvCopyParam,
                packagesDirectory: packageDir,
                disableCollectStatic: disableCollectStatic).TransformText();

            return new BuildScriptSnippet()
            {
                BashBuildScriptSnippet = script,
                RequiredToolsVersion = new Dictionary<string, string> { { "python", pythonVersion } }
            };
        }
    }
}