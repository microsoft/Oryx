// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Resources;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    [BuildProperty(VirtualEnvironmentNamePropertyKey, "If provided, will create a virtual environment with the given name.")]
    [BuildProperty(TargetPackageDirectoryPropertyKey, "Directory to download the packages to, if no virtual environment is provided. Default: '" + DefaultTargetPackageDirectory + "'")]
    internal class PythonPlatform : IProgrammingPlatform
    {
        internal const string VirtualEnvironmentNamePropertyKey = "virtualenv_name";
        internal const string TargetPackageDirectoryPropertyKey = "packagedir";

        private const string PythonName = "python";
        private const string DefaultTargetPackageDirectory = "__oryx_packages__";

        private readonly PythonScriptGeneratorOptions _pythonScriptGeneratorOptions;
        private readonly IPythonVersionProvider _pythonVersionProvider;
        private readonly IEnvironment _environment;
        private readonly ILogger<PythonPlatform> _logger;
        private readonly PythonLanguageDetector _detector;

        public PythonPlatform(
            IOptions<PythonScriptGeneratorOptions> pythonScriptGeneratorOptions,
            IPythonVersionProvider pythonVersionProvider,
            IEnvironment environment,
            ILogger<PythonPlatform> logger,
            PythonLanguageDetector detector)
        {
            _pythonScriptGeneratorOptions = pythonScriptGeneratorOptions.Value;
            _pythonVersionProvider = pythonVersionProvider;
            _environment = environment;
            _logger = logger;
            _detector = detector;
        }

        public string Name => PythonName;

        public IEnumerable<string> SupportedLanguageVersions => _pythonVersionProvider.SupportedPythonVersions;

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            return _detector.Detect(sourceRepo);
        }

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

            var pythonVersion = context.PythonVersion;
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

            TryLogDependencies(pythonVersion, context.SourceRepo);

            var scriptProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: virtualEnvName,
                virtualEnvironmentModule: virtualEnvModule,
                virtualEnvironmentParameters: virtualEnvCopyParam,
                packagesDirectory: packageDir,
                disableCollectStatic: disableCollectStatic);
            string script = TemplateHelpers.Render(TemplateHelpers.TemplateResource.PythonSnippet, scriptProps, _logger);

            return new BuildScriptSnippet()
            {
                BashBuildScriptSnippet = script,
            };
        }

        public bool IsEnabled(ScriptGeneratorContext scriptGeneratorContext)
        {
            return scriptGeneratorContext.EnablePython;
        }

        public void SetRequiredTools(ISourceRepo sourceRepo, string targetPlatformVersion, IDictionary<string, string> toolsToVersion)
        {
            Debug.Assert(toolsToVersion != null, $"{nameof(toolsToVersion)} must not be null");
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion[PythonName] = targetPlatformVersion;
            }
        }

        public void SetVersion(ScriptGeneratorContext context, string version)
        {
            context.PythonVersion = version;
        }

        private void TryLogDependencies(string pythonVersion, ISourceRepo repo)
        {
            if (!repo.FileExists(PythonConstants.RequirementsFileName))
            {
                return;
            }

            try
            {
                var deps = repo.ReadAllLines(PythonConstants.RequirementsFileName)
                    .Where(line => !line.TrimStart().StartsWith("#"));
                _logger.LogDependencies(PythonName, pythonVersion, deps);
            }
            catch (Exception exc)
            {
                _logger.LogWarning(exc, "Exception caught while logging dependencies");
            }
        }
    }
}