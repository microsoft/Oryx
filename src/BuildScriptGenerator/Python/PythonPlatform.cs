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
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    /// <summary>
    /// Python Platform.
    /// </summary>
    [BuildProperty(
        VirtualEnvironmentNamePropertyKey,
        "Name of the virtual environment to be created. Defaults to 'pythonenv<Python version>'.")]
    [BuildProperty(
        CompressVirtualEnvPropertyKey,
        "Indicates how and if virtual environment folder should be compressed into a single file in the output " +
        "folder. Options are '" + ZipOption + "', and '" + TarGzOption + "'. Default is to not compress. " +
        "If this option is used, when running the app the virtual environment folder must be extracted from " +
        "this file.")]
    [BuildProperty(
        TargetPackageDirectoryPropertyKey,
        "If provided, packages will be downloaded to the given directory instead of to a virtual environment.")]
    internal class PythonPlatform : IProgrammingPlatform
    {
        /// <summary>
        /// The name of virtual environment.
        /// </summary>
        internal const string VirtualEnvironmentNamePropertyKey = "virtualenv_name";

        /// <summary>
        /// The target package directory.
        /// </summary>
        internal const string TargetPackageDirectoryPropertyKey = "packagedir";

        /// <summary>
        /// The compress virtual environment.
        /// </summary>
        internal const string CompressVirtualEnvPropertyKey = "compress_virtualenv";

        /// <summary>
        /// The zip option.
        /// </summary>
        internal const string ZipOption = "zip";

        /// <summary>
        /// The tar-gz option.
        /// </summary>
        internal const string TarGzOption = "tar-gz";

        private readonly IPythonVersionProvider _pythonVersionProvider;
        private readonly IEnvironment _environment;
        private readonly ILogger<PythonPlatform> _logger;
        private readonly PythonLanguageDetector _detector;

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonPlatform"/> class.
        /// </summary>
        /// <param name="pythonScriptGeneratorOptions">The options of pythonScriptGenerator.</param>
        /// <param name="pythonVersionProvider">The Python version provider.</param>
        /// <param name="environment">The environment of Python platform.</param>
        /// <param name="logger">The logger of Python platform.</param>
        /// <param name="detector">The detector of Python platform.</param>
        public PythonPlatform(
            IOptions<PythonScriptGeneratorOptions> pythonScriptGeneratorOptions,
            IPythonVersionProvider pythonVersionProvider,
            IEnvironment environment,
            ILogger<PythonPlatform> logger,
            PythonLanguageDetector detector)
        {
            _pythonVersionProvider = pythonVersionProvider;
            _environment = environment;
            _logger = logger;
            _detector = detector;
        }

        /// <summary>
        /// Gets the name of Python platform which this generator will create builds for.
        /// </summary>
        public string Name => PythonConstants.PythonName;

        /// <summary>
        /// Gets the list of versions that the script generator supports.
        /// </summary>
        public IEnumerable<string> SupportedVersions => _pythonVersionProvider.SupportedPythonVersions;

        /// <summary>
        /// Detects the programming platform name and version required by the application in source directory.
        /// </summary>
        /// <param name="context">The repository context.</param>
        /// <returns>The results of language detector operations.</returns>
        public LanguageDetectorResult Detect(RepositoryContext context)
        {
            return _detector.Detect(context);
        }

        /// <summary>
        /// Generates a build Bash script based on the application in source directory.
        /// </summary>
        /// <param name="context">The context for BuildScriptGenerator.</param>
        /// <returns>The build script snippet.</returns>
        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext context)
        {
            var manifestFileProperties = new Dictionary<string, string>();

            // Write the version to the manifest file
            var key = $"{PythonConstants.PythonName}_version";
            manifestFileProperties[key] = context.PythonVersion;

            var packageDir = GetPackageDirectory(context);
            var virtualEnvName = GetVirtualEnvironmentName(context);

            if (!string.IsNullOrWhiteSpace(packageDir) && !string.IsNullOrWhiteSpace(virtualEnvName))
            {
                throw new InvalidUsageException($"Options '{TargetPackageDirectoryPropertyKey}' and " +
                    $"'{VirtualEnvironmentNamePropertyKey}' are mutually exclusive. Please provide " +
                    $"only the target package directory or virtual environment name.");
            }

            if (string.IsNullOrWhiteSpace(packageDir))
            {
                // If the package directory was not provided, we default to virtual envs
                if (string.IsNullOrWhiteSpace(virtualEnvName))
                {
                    virtualEnvName = GetDefaultVirtualEnvName(context);
                }

                manifestFileProperties[ManifestFilePropertyKeys.VirtualEnvName] = virtualEnvName;
            }
            else
            {
                manifestFileProperties[ManifestFilePropertyKeys.PackageDir] = packageDir;
            }

            var virtualEnvModule = string.Empty;
            var virtualEnvCopyParam = string.Empty;

            var pythonVersion = context.PythonVersion;
            _logger.LogDebug("Selected Python version: {pyVer}", pythonVersion);

            if (!string.IsNullOrEmpty(pythonVersion) && !string.IsNullOrWhiteSpace(virtualEnvName))
            {
                (virtualEnvModule, virtualEnvCopyParam) = GetVirtualEnvModules(pythonVersion);

                _logger.LogDebug(
                    "Using virtual environment {venv}, module {venvModule}",
                    virtualEnvName,
                    virtualEnvModule);
            }

            bool enableCollectStatic = IsCollectStaticEnabled();

            GetVirtualEnvPackOptions(
                context,
                virtualEnvName,
                out var compressVirtualEnvCommand,
                out var compressedVirtualEnvFileName);

            if (!string.IsNullOrWhiteSpace(compressedVirtualEnvFileName))
            {
                manifestFileProperties[ManifestFilePropertyKeys.CompressedVirtualEnvFile]
                    = compressedVirtualEnvFileName;
            }

            TryLogDependencies(pythonVersion, context.SourceRepo);

            var scriptProps = new PythonBashBuildSnippetProperties(
                virtualEnvironmentName: virtualEnvName,
                virtualEnvironmentModule: virtualEnvModule,
                virtualEnvironmentParameters: virtualEnvCopyParam,
                packagesDirectory: packageDir,
                disableCollectStatic: !enableCollectStatic,
                compressVirtualEnvCommand: compressVirtualEnvCommand,
                compressedVirtualEnvFileName: compressedVirtualEnvFileName);
            string script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.PythonSnippet,
                scriptProps,
                _logger);

            return new BuildScriptSnippet()
            {
                BashBuildScriptSnippet = script,
                BuildProperties = manifestFileProperties,
            };
        }

        /// <summary>
        /// Checks if the source repository seems to have artifacts from a previous build.
        /// </summary>
        /// <param name="repo">A source code repository.</param>
        /// <returns>True if the source repository have artifacts already, False otherwise.</returns>
        public bool IsCleanRepo(ISourceRepo repo)
        {
            // TODO: support venvs
            return !repo.DirExists(PythonConstants.DefaultTargetPackageDirectory);
        }

        /// <summary>
        /// Generates a bash script that can install the required runtime bits for the application's platforms.
        /// </summary>
        /// <param name="options">The runtime installation script generator options.</param>
        /// <exception cref="NotImplementedException">Thrown when it's not implemented.</exception>
        /// <returns>Message from exception.</returns>
        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the programming platform should be included in a build script.
        /// </summary>
        /// <param name="ctx">The repository context.</param>
        /// <returns>True if the programming platform should be included in a build script, False otherwise.</returns>
        public bool IsEnabled(RepositoryContext ctx)
        {
            return ctx.EnablePython;
        }

        /// <summary>
        /// Checks if the programming platform wants to participate in a multi-platform build.
        /// </summary>
        /// <param name="ctx">The repository context.</param>
        /// <returns>True if the programming platform is enabled for multi-platform build, False otherwise.</returns>
        public bool IsEnabledForMultiPlatformBuild(RepositoryContext ctx)
        {
            return true;
        }

        /// <summary>
        /// Adds the required tools and their versions to a map.
        /// </summary>
        /// <param name="sourceRepo">The source repository.</param>
        /// <param name="targetPlatformVersion">The version of target platform.</param>
        /// <param name="toolsToVersion">A dictionary with tools as keys and versions as values.</param>
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

        /// <summary>
        /// Sets the version of the Python platform in BuildScriptGeneratorContext.
        /// </summary>
        /// <param name="context">The context of BuildScriptGenerator.</param>
        /// <param name="version">The version of the Python platform.</param>
        public void SetVersion(BuildScriptGeneratorContext context, string version)
        {
            context.PythonVersion = version;
        }

        /// <summary>
        /// Gets list of directories which need to be excluded from being copied to the output directory.
        /// </summary>
        /// <param name="context">The context of BuildScriptGenerator.</param>
        /// <returns>A list of directories.</returns>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(
            BuildScriptGeneratorContext context)
        {
            var dirs = new List<string>();
            var virtualEnvName = GetVirtualEnvironmentName(context);
            if (GetVirtualEnvPackOptions(
                context,
                virtualEnvName,
                out _,
                out string compressedFileName))
            {
                dirs.Add(virtualEnvName);
            }
            else if (!string.IsNullOrWhiteSpace(compressedFileName))
            {
                dirs.Add(compressedFileName);
            }

            return dirs;
        }

        /// <summary>
        /// Gets list of directories which need to be excluded from being copied to the intermediate directory, if used.
        /// </summary>
        /// <param name="context">The context of BuildScriptGenerator.</param>
        /// <returns>A list of directories.</returns>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(
            BuildScriptGeneratorContext context)
        {
            var excludeDirs = new List<string>();

            excludeDirs.Add(PythonConstants.DefaultTargetPackageDirectory);

            var virtualEnvName = GetVirtualEnvironmentName(context);
            if (!string.IsNullOrEmpty(virtualEnvName))
            {
                excludeDirs.Add(virtualEnvName);
                excludeDirs.Add(string.Format(PythonConstants.ZipVirtualEnvFileNameFormat, virtualEnvName));
                excludeDirs.Add(string.Format(PythonConstants.TarGzVirtualEnvFileNameFormat, virtualEnvName));
            }

            return excludeDirs;
        }

        private static string GetDefaultVirtualEnvName(BuildScriptGeneratorContext context)
        {
            string pythonVersion = context.PythonVersion;
            if (!string.IsNullOrWhiteSpace(pythonVersion))
            {
                var versionSplit = pythonVersion.Split('.');
                if (versionSplit.Length > 1)
                {
                    pythonVersion = $"{versionSplit[0]}.{versionSplit[1]}";
                }
            }

            return $"pythonenv{pythonVersion}";
        }

        private static string GetPackageDirectory(BuildScriptGeneratorContext context)
        {
            string packageDir = null;
            if (context.Properties != null)
            {
                context.Properties.TryGetValue(TargetPackageDirectoryPropertyKey, out packageDir);
            }

            return packageDir;
        }

        private static bool GetVirtualEnvPackOptions(
            BuildScriptGeneratorContext context,
            string virtualEnvName,
            out string compressVirtualEnvCommand,
            out string compressedVirtualEnvFileName)
        {
            var isVirtualEnvPackaged = false;
            compressVirtualEnvCommand = null;
            compressedVirtualEnvFileName = null;
            if (context.Properties != null &&
                context.Properties.TryGetValue(CompressVirtualEnvPropertyKey, out string compressVirtualEnvOption))
            {
                // default to tar.gz if the property was provided with no value.
                if (string.IsNullOrEmpty(compressVirtualEnvOption) ||
                    compressVirtualEnvOption.EqualsIgnoreCase(TarGzOption))
                {
                    compressedVirtualEnvFileName = string.Format(
                        PythonConstants.TarGzVirtualEnvFileNameFormat,
                        virtualEnvName);
                    compressVirtualEnvCommand = $"tar -zcf";
                    isVirtualEnvPackaged = true;
                }
                else if (compressVirtualEnvOption.EqualsIgnoreCase(ZipOption))
                {
                    compressedVirtualEnvFileName = string.Format(
                        PythonConstants.ZipVirtualEnvFileNameFormat,
                        virtualEnvName);
                    compressVirtualEnvCommand = $"zip -y -q -r";
                    isVirtualEnvPackaged = true;
                }
            }

            return isVirtualEnvPackaged;
        }

        private bool IsCollectStaticEnabled()
        {
            // Collect static is enabled by default, but users can opt-out of it
            var enableCollectStatic = true;
            var disableCollectStaticEnvValue = _environment.GetEnvironmentVariable(
                EnvironmentSettingsKeys.DisableCollectStatic);
            if (disableCollectStaticEnvValue.EqualsIgnoreCase(Constants.True))
            {
                enableCollectStatic = false;
            }

            return enableCollectStatic;
        }

        private (string virtualEnvModule, string virtualEnvCopyParam) GetVirtualEnvModules(string pythonVersion)
        {
            string virtualEnvModule;
            string virtualEnvCopyParam = string.Empty;
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

            return (virtualEnvModule, virtualEnvCopyParam);
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

        private string GetVirtualEnvironmentName(BuildScriptGeneratorContext context)
        {
            if (context.Properties == null ||
                !context.Properties.TryGetValue(VirtualEnvironmentNamePropertyKey, out var virtualEnvName))
            {
                virtualEnvName = string.Empty;
            }

            return virtualEnvName;
        }
    }
}