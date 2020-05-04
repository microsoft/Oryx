// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// .NET Core platform.
    /// </summary>
    [BuildProperty(
        DotNetCoreConstants.ProjectBuildPropertyKey,
        DotNetCoreConstants.ProjectBuildPropertyKeyDocumentation)]
    internal class DotNetCorePlatform : IProgrammingPlatform
    {
        private readonly IDotNetCoreVersionProvider _versionProvider;
        private readonly DefaultProjectFileProvider _projectFileProvider;
        private readonly ILogger<DotNetCorePlatform> _logger;
        private readonly DotNetCorePlatformDetector _detector;
        private readonly DotNetCoreScriptGeneratorOptions _dotNetCoreScriptGeneratorOptions;
        private readonly BuildScriptGeneratorOptions _cliOptions;
        private readonly IEnvironment _environment;
        private readonly DotNetCorePlatformInstaller _platformInstaller;
        private readonly GlobalJsonSdkResolver _globalJsonSdkResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetCorePlatform"/> class.
        /// </summary>
        /// <param name="versionProvider">The .NET version provider.</param>
        /// <param name="projectFileProvider">The project file provider.</param>
        /// <param name="environmentSettingsProvider">The environment settings provider.</param>
        /// <param name="logger">The logger of .NET platform.</param>
        /// <param name="detector">The detector of .NET platform.</param>
        /// <param name="cliOptions">The build options for BuildScriptGenerator.</param>
        /// <param name="dotNetCoreScriptGeneratorOptions">The options if .NET platform.</param>
        /// <param name="platformInstaller">The <see cref="DotNetCorePlatformInstaller"/>.</param>
        public DotNetCorePlatform(
            IDotNetCoreVersionProvider versionProvider,
            DefaultProjectFileProvider projectFileProvider,
            ILogger<DotNetCorePlatform> logger,
            DotNetCorePlatformDetector detector,
            IOptions<BuildScriptGeneratorOptions> cliOptions,
            IOptions<DotNetCoreScriptGeneratorOptions> dotNetCoreScriptGeneratorOptions,
            IEnvironment environment,
            DotNetCorePlatformInstaller platformInstaller,
            GlobalJsonSdkResolver globalJsonSdkResolver)
        {
            _versionProvider = versionProvider;
            _projectFileProvider = projectFileProvider;
            _logger = logger;
            _detector = detector;
            _dotNetCoreScriptGeneratorOptions = dotNetCoreScriptGeneratorOptions.Value;
            _cliOptions = cliOptions.Value;
            _environment = environment;
            _platformInstaller = platformInstaller;
            _globalJsonSdkResolver = globalJsonSdkResolver;
        }

        /// <inheritdoc/>
        public string Name => DotNetCoreConstants.PlatformName;

        /// <inheritdoc/>
        public IEnumerable<string> SupportedVersions
        {
            get
            {
                var versionMap = _versionProvider.GetSupportedVersions();

                // Map is from runtime version => sdk version
                return versionMap.Keys;
            }
        }

        /// <inheritdoc/>
        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            return _detector.Detect(context);
        }

        /// <inheritdoc/>
        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext context)
        {
            var versionMap = _versionProvider.GetSupportedVersions();

            string installationScriptSnippet = null;
            string globalJsonSdkVersion = null;
            if (_cliOptions.EnableDynamicInstall)
            {
                _logger.LogDebug("Dynamic install is enabled.");

                var availableSdks = versionMap.Values;
                globalJsonSdkVersion = _globalJsonSdkResolver.GetSatisfyingSdkVersion(
                    context.SourceRepo,
                    context.ResolvedDotNetCoreRuntimeVersion,
                    availableSdks);

                if (_platformInstaller.IsVersionAlreadyInstalled(
                    context.ResolvedDotNetCoreRuntimeVersion,
                    globalJsonSdkVersion))
                {
                    _logger.LogDebug(
                        "DotNetCore runtime version {runtimeVersion} is already installed. " +
                        "So skipping installing it again.",
                        context.ResolvedDotNetCoreRuntimeVersion);
                }
                else
                {
                    _logger.LogDebug(
                        "DotNetCore runtime version {runtimeVersion} is not installed. " +
                        "So generating an installation script snippet for it.",
                        context.ResolvedDotNetCoreRuntimeVersion);

                    installationScriptSnippet = _platformInstaller.GetInstallerScriptSnippet(
                        context.ResolvedDotNetCoreRuntimeVersion,
                        globalJsonSdkVersion);
                }
            }
            else
            {
                _logger.LogDebug("Dynamic install is not enabled.");
            }

            var manifestFileProperties = new Dictionary<string, string>();
            manifestFileProperties[ManifestFilePropertyKeys.OperationId] = context.OperationId;
            manifestFileProperties[ManifestFilePropertyKeys.DotNetCoreRuntimeVersion]
                = context.ResolvedDotNetCoreRuntimeVersion;

            if (string.IsNullOrEmpty(globalJsonSdkVersion))
            {
                manifestFileProperties[ManifestFilePropertyKeys.DotNetCoreSdkVersion]
                    = versionMap[context.ResolvedDotNetCoreRuntimeVersion];
            }
            else
            {
                manifestFileProperties[ManifestFilePropertyKeys.DotNetCoreSdkVersion] = globalJsonSdkVersion;
            }

            var projectFile = _projectFileProvider.GetRelativePathToProjectFile(context);
            if (string.IsNullOrEmpty(projectFile))
            {
                return null;
            }

            var templateProperties = new DotNetCoreBashBuildSnippetProperties
            {
                ProjectFile = projectFile,
                Configuration = GetBuildConfiguration(),
            };

            var script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.DotNetCoreSnippet,
                templateProperties,
                _logger);

            SetStartupFileNameInfoInManifestFile(context, projectFile, manifestFileProperties);

            return new BuildScriptSnippet
            {
                BashBuildScriptSnippet = script,
                BuildProperties = manifestFileProperties,
                PlatformInstallationScriptSnippet = installationScriptSnippet,

                // Setting this to false to avoid copying files like '.cs' to the destination
                CopySourceDirectoryContentToDestinationDirectory = false,
            };
        }

        /// <inheritdoc/>
        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        /// <inheritdoc/>
        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool IsEnabled(RepositoryContext ctx)
        {
            return _cliOptions.EnableDotNetCoreBuild;
        }

        /// <inheritdoc/>
        public bool IsEnabledForMultiPlatformBuild(RepositoryContext ctx)
        {
            return true;
        }

        /// <inheritdoc/>
        public void SetRequiredTools(
            ISourceRepo sourceRepo,
            string targetPlatformVersion,
            IDictionary<string, string> toolsToVersion)
        {
            Debug.Assert(toolsToVersion != null, $"{nameof(toolsToVersion)} must not be null.");
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion[ToolNameConstants.DotNetName] = targetPlatformVersion;
            }
        }

        /// <inheritdoc/>
        public void SetVersion(BuildScriptGeneratorContext context, string runtimeVersion)
        {
            context.ResolvedDotNetCoreRuntimeVersion = runtimeVersion;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            var dirs = new List<string>();
            dirs.Add("obj");
            dirs.Add("bin");
            return dirs;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            var dirs = new List<string>();
            dirs.Add(".git");
            dirs.Add("obj");
            dirs.Add("bin");
            return dirs;
        }

        public string GetMaxSatisfyingVersionAndVerify(string runtimeVersion)
        {
            return _detector.GetMaxSatisfyingVersionAndVerify(runtimeVersion);
        }

        private string GetBuildConfiguration()
        {
            var configuration = _dotNetCoreScriptGeneratorOptions.MSBuildConfiguration;
            if (string.IsNullOrEmpty(configuration))
            {
                configuration = DotNetCoreConstants.DefaultMSBuildConfiguration;
            }

            return configuration;
        }

        /// <summary>
        /// Even though the runtime container has the logic of finding out the startup file based on
        /// 'runtimeconfig.json' prefix, we still set the name in the manifest file because of the following
        /// scenario: let's say output directory currently has 'foo.dll' and user made a change to the project
        /// name or assembly name property to 'bar' which causes 'bar.dll' to be published. If the output
        /// directory was NOT cleaned, then we would now be having both 'foo.runtimeconfig.json' and
        /// 'bar.runtimeconfig.json' which causes a problem for runtime container as it cannot figure out the
        /// right startup DLL. So, to help that scenario we always set the start-up file name in manifest file.
        /// The runtime container will first look into manifest file to find the startup filename, if the
        /// file name is not present or if a manifest file is not present at all(ex: in case of VS Publish where
        /// the build does not happen with Oryx), then the runtime container's logic will fallback to looking at
        /// runtimeconfig.json prefixes.
        /// </summary>
        private void SetStartupFileNameInfoInManifestFile(
            BuildScriptGeneratorContext context,
            string projectFile,
            IDictionary<string, string> buildProperties)
        {
            string startupDllFileName;
            var projectFileContent = context.SourceRepo.ReadFile(projectFile);
            var projFileDoc = XDocument.Load(new StringReader(projectFileContent));
            var assemblyNameElement = projFileDoc.XPathSelectElement(DotNetCoreConstants.AssemblyNameXPathExpression);
            if (assemblyNameElement == null)
            {
                var name = Path.GetFileNameWithoutExtension(projectFile);
                startupDllFileName = $"{name}.dll";
            }
            else
            {
                startupDllFileName = $"{assemblyNameElement.Value}.dll";
            }

            buildProperties[DotNetCoreManifestFilePropertyKeys.StartupDllFileName] = startupDllFileName;
        }
    }
}