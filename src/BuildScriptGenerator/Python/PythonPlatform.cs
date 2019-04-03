// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    [BuildProperty(
        PythonConstants.VirtualEnvironmentNamePropertyKey,
        "If provided, will create a virtual environment with the given name.")]
    [BuildProperty(
        PythonConstants.ZipVenvDirPropertyKey,
        "If provided, the virtual environment folder will be zipped to the output folder.")]
    [BuildProperty(
        PythonConstants.TargetPackageDirectoryPropertyKey,
        "Directory to download the packages to, if no virtual environment is provided. Default: '" +
        PythonConstants.DefaultTargetPackageDirectory + "'")]
    [BuildProperty(
        Constants.ZipAllOutputBuildPropertyKey,
        "Zips entire output content and puts the file in the destination directory." +
        "Options are 'true', blank (same meaning as 'true'), and 'false'. Default is false.")]
    internal class PythonPlatform : IProgrammingPlatform
    {
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

        public string Name => PythonConstants.PythonName;

        public IEnumerable<string> SupportedLanguageVersions => _pythonVersionProvider.SupportedPythonVersions;

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            return _detector.Detect(sourceRepo);
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext context)
        {
            var buildProperties = new Dictionary<string, string>();
            var virtualEnvName = GetVirutalEnvironmentName(context);

            string packageDir = null;
            if (context.Properties == null || !context.Properties.TryGetValue(
                PythonConstants.TargetPackageDirectoryPropertyKey,
                out packageDir))
            {
                packageDir = PythonConstants.DefaultTargetPackageDirectory;
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

            var exlcudedDirs = GetDirectoriesToExcludeFromCopyToBuildOutputDir(context);
            var zipAllOutput = ShouldZipAllOutput(context);

            var scriptProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: virtualEnvName,
                virtualEnvironmentModule: virtualEnvModule,
                virtualEnvironmentParameters: virtualEnvCopyParam,
                packagesDirectory: packageDir,
                disableCollectStatic: disableCollectStatic,
                zipVirtualEnvDir: ShouldZipVenvDir(context),
                directoriesToExcludeFromCopyToBuildOutputDir: exlcudedDirs,
                zipAllOutput: zipAllOutput);
            string script = TemplateHelpers.Render(
                TemplateHelpers.TemplateResource.PythonSnippet,
                scriptProps,
                _logger);

            return new BuildScriptSnippet()
            {
                BashBuildScriptSnippet = script,
            };
        }

        public bool IsCleanRepo(ISourceRepo repo)
        {
            // TODO: support venvs
            return !repo.DirExists(PythonConstants.DefaultTargetPackageDirectory);
        }

        public string GenerateBashRunScript(RunScriptGeneratorOptions runScriptGeneratorOptions)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return scriptGeneratorContext.EnablePython;
        }

        public void SetRequiredTools(
            ISourceRepo sourceRepo,
            string targetPlatformVersion,
            [NotNull] IDictionary<string, string> toolsToVersion)
        {
            Debug.Assert(toolsToVersion != null, $"{nameof(toolsToVersion)} must not be null");
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion[PythonConstants.PythonName] = targetPlatformVersion;
            }
        }

        public void SetVersion(BuildScriptGeneratorContext context, string version)
        {
            context.PythonVersion = version;
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(BuildScriptGeneratorContext context)
        {
            var zipAllOutput = ShouldZipAllOutput(context);
            if (!zipAllOutput)
            {
                var venvName = GetVirutalEnvironmentName(context);
                if (string.IsNullOrEmpty(venvName))
                {
                    return Array.Empty<string>();
                }

                string dir = ShouldZipVenvDir(context) ? venvName : $"{venvName}.{PythonConstants.ZipFileExtension}";
                return new string[] { dir };
            }

            return Array.Empty<string>();
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(
            BuildScriptGeneratorContext context)
        {
            var excludeDirs = new List<string>();
            excludeDirs.Add(PythonConstants.DefaultTargetPackageDirectory);

            var virtualEnvName = GetVirutalEnvironmentName(context);
            if (!string.IsNullOrEmpty(virtualEnvName))
            {
                excludeDirs.Add($"{virtualEnvName}.{PythonConstants.ZipFileExtension}");
            }

            return excludeDirs;
        }

        private static bool ShouldZipAllOutput(BuildScriptGeneratorContext context)
        {
            return BuildPropertiesHelper.IsTrue(
                Constants.ZipAllOutputBuildPropertyKey,
                context,
                valueIsRequired: false);
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
                _logger.LogDependencies(PythonConstants.PythonName, pythonVersion, deps);
            }
            catch (Exception exc)
            {
                _logger.LogWarning(exc, "Exception caught while logging dependencies");
            }
        }

        private string GetVirutalEnvironmentName(BuildScriptGeneratorContext context)
        {
            if (context.Properties == null ||
                !context.Properties.TryGetValue(
                    PythonConstants.VirtualEnvironmentNamePropertyKey,
                    out var virtualEnvName))
            {
                virtualEnvName = string.Empty;
            }

            return virtualEnvName;
        }

        private bool ShouldZipVenvDir(BuildScriptGeneratorContext context)
        {
            // Build property takes precedence over env var
            return context.Properties?.ContainsKey(PythonConstants.ZipVenvDirPropertyKey) == true ||
                _pythonScriptGeneratorOptions.ZipVirtualEnvDir;
        }
    }
}