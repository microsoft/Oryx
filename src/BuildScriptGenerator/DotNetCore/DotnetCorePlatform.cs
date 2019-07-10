// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// .NET Core platform.
    /// </summary>
    [BuildProperty(Constants.ZipAllOutputBuildPropertyKey, Constants.ZipAllOutputBuildPropertyKeyDocumentation)]
    internal class DotNetCorePlatform : IProgrammingPlatform
    {
        private readonly IDotNetCoreVersionProvider _versionProvider;
        private readonly IAspNetCoreWebAppProjectFileProvider _aspNetCoreWebAppProjectFileProvider;
        private readonly IEnvironmentSettingsProvider _environmentSettingsProvider;
        private readonly ILogger<DotNetCorePlatform> _logger;
        private readonly DotNetCoreLanguageDetector _detector;
        private readonly DotNetCoreScriptGeneratorOptions _options;

        public DotNetCorePlatform(
            IDotNetCoreVersionProvider versionProvider,
            IAspNetCoreWebAppProjectFileProvider aspNetCoreWebAppProjectFileProvider,
            IEnvironmentSettingsProvider environmentSettingsProvider,
            ILogger<DotNetCorePlatform> logger,
            DotNetCoreLanguageDetector detector,
            IOptions<DotNetCoreScriptGeneratorOptions> options)
        {
            _versionProvider = versionProvider;
            _aspNetCoreWebAppProjectFileProvider = aspNetCoreWebAppProjectFileProvider;
            _environmentSettingsProvider = environmentSettingsProvider;
            _logger = logger;
            _detector = detector;
            _options = options.Value;
        }

        public string Name => DotNetCoreConstants.LanguageName;

        public IEnumerable<string> SupportedVersions => _versionProvider.SupportedDotNetCoreVersions;

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            return _detector.Detect(sourceRepo);
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext context)
        {
            var buildProperties = new Dictionary<string, string>();
            buildProperties[ManifestFilePropertyKeys.OperationId] = context.OperationId;

            (string projectFile, string publishDir) = GetProjectFileAndPublishDir(context.SourceRepo);
            if (string.IsNullOrEmpty(projectFile) || string.IsNullOrEmpty(publishDir))
            {
                return null;
            }

            SetStartupFileNameInfoInManifestFile(context, projectFile, buildProperties);

            bool zipAllOutput = ShouldZipAllOutput(context);
            buildProperties[ManifestFilePropertyKeys.ZipAllOutput] = zipAllOutput.ToString().ToLowerInvariant();

            _environmentSettingsProvider.TryGetAndLoadSettings(out var environmentSettings);

            (var preBuildCommand, var postBuildCommand) = PreAndPostBuildCommandHelper.GetPreAndPostBuildCommands(
                context.SourceRepo,
                environmentSettings);

            var templateProperties = new DotNetCoreBashBuildSnippetProperties
            {
                ProjectFile = projectFile,
                PublishDirectory = publishDir,
                BuildProperties = buildProperties,
                BenvArgs = $"dotnet={context.DotNetCoreVersion}",
                DirectoriesToExcludeFromCopyToIntermediateDir = GetDirectoriesToExcludeFromCopyToIntermediateDir(
                    context),
                PreBuildCommand = preBuildCommand,
                PostBuildCommand = postBuildCommand,
                ManifestFileName = FilePaths.BuildManifestFileName,
                ManifestDir = context.ManifestDir,
                ZipAllOutput = zipAllOutput,
                Configuration = GetBuildConfiguration(),
            };
            var script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.DotNetCoreSnippet,
                templateProperties,
                _logger);
            return new BuildScriptSnippet { BashBuildScriptSnippet = script, IsFullScript = true };
        }

        /// <summary>
        /// Even though the runtime container has the logic of finding out the startup file based on
        /// 'runtimeconfig.json' prefix, we still set the name in the manifest file because of the following
        /// scenario: let's say output directory currently has 'foo.dll' and user made a change to the project
        /// name or assembly name property to 'bar' which causes 'bar.dll' to be published. If the output
        /// directory was NOT cleaned, then we would now be having both 'foo.runtimeconfig.json' and
        /// 'bar.runtimeconfig.json' which causes a problem for runtime container as it cannot figure out the
        /// right startup dll. So, to help that scenario we always set the start-up file name in manifest file.
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

        public bool IsCleanRepo(ISourceRepo repo)
        {
            (_, string expectedPublishDir) = GetProjectFileAndPublishDir(repo);
            return !repo.DirExists(expectedPublishDir);
        }

        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return scriptGeneratorContext.EnableDotNetCore;
        }

        public bool IsEnabledForMultiPlatformBuild(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            // A user has the power to either enable or disable multi-platform builds entirely.
            // However if user enables it, ASP.NET Core platform still explicitly opts out of it.
            return false;
        }

        public void SetRequiredTools(
            ISourceRepo sourceRepo,
            string targetPlatformVersion,
            IDictionary<string, string> toolsToVersion)
        {
            Debug.Assert(toolsToVersion != null, $"{nameof(toolsToVersion)} must not be null.");
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion["dotnet"] = targetPlatformVersion;
            }
        }

        public void SetVersion(BuildScriptGeneratorContext context, string version)
        {
            context.DotNetCoreVersion = version;
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            var dirs = new List<string>();
            dirs.Add("obj");
            dirs.Add("bin");
            return dirs;
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            var dirs = new List<string>();
            dirs.Add(".git");
            dirs.Add("obj");
            dirs.Add("bin");
            dirs.Add(DotNetCoreConstants.OryxOutputPublishDirectory);
            return dirs;
        }

        private static bool ShouldZipAllOutput(BuildScriptGeneratorContext context)
        {
            return BuildPropertiesHelper.IsTrue(
                Constants.ZipAllOutputBuildPropertyKey,
                context,
                valueIsRequired: false);
        }

        private string GetBuildConfiguration()
        {
            var configuration = _options.MSBuildConfiguration;
            if (string.IsNullOrEmpty(configuration))
            {
                configuration = DotNetCoreConstants.DefaultMSBuildConfiguration;
            }

            return configuration;
        }

        private (string projFile, string publishDir) GetProjectFileAndPublishDir(ISourceRepo repo)
        {
            var projectFile = _aspNetCoreWebAppProjectFileProvider.GetRelativePathToProjectFile(repo);
            if (string.IsNullOrEmpty(projectFile))
            {
                return (null, null);
            }

            var publishDir = Path.Combine(repo.RootPath, DotNetCoreConstants.OryxOutputPublishDirectory);
            return (projectFile, publishDir);
        }
    }
}