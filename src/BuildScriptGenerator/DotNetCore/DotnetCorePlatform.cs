// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    [BuildProperty(
        Constants.ZipAllOutputBuildPropertyKey,
        "Zips entire output content and puts the file in the destination directory." +
        "Options are 'true', blank (same meaning as 'true'), and 'false'. Default is false.")]
    internal class DotnetCorePlatform : IProgrammingPlatform
    {
        private readonly IDotnetCoreVersionProvider _versionProvider;
        private readonly IAspNetCoreWebAppProjectFileProvider _aspNetCoreWebAppProjectFileProvider;
        private readonly ILogger<DotnetCorePlatform> _logger;
        private readonly DotnetCoreLanguageDetector _detector;

        public DotnetCorePlatform(
            IDotnetCoreVersionProvider versionProvider,
            IAspNetCoreWebAppProjectFileProvider aspNetCoreWebAppProjectFileProvider,
            ILogger<DotnetCorePlatform> logger,
            DotnetCoreLanguageDetector detector)
        {
            _versionProvider = versionProvider;
            _aspNetCoreWebAppProjectFileProvider = aspNetCoreWebAppProjectFileProvider;
            _logger = logger;
            _detector = detector;
        }

        public string Name => DotnetCoreConstants.LanguageName;

        public IEnumerable<string> SupportedLanguageVersions => _versionProvider.SupportedDotNetCoreVersions;

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            return _detector.Detect(sourceRepo);
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext context)
        {
            var buildProperties = new Dictionary<string, string>();
            (string projectFile, string publishDir) = GetProjectFileAndPublishDir(context.SourceRepo);
            if (string.IsNullOrEmpty(projectFile) || string.IsNullOrEmpty(publishDir))
            {
                return null;
            }

            string startupFileName = null;
            var projectFileContent = context.SourceRepo.ReadFile(projectFile);
            var projFileDoc = XDocument.Load(new StringReader(projectFileContent));
            var assemblyNameElement = projFileDoc.XPathSelectElement("/Project/PropertyGroup/AssemblyName");
            if (assemblyNameElement == null)
            {
                var name = Path.GetFileNameWithoutExtension(projectFile);
                startupFileName = $"{name}.dll";
            }
            else
            {
                startupFileName = $"{assemblyNameElement.Value}.dll";
            }

            buildProperties[DotnetCoreConstants.StartupFileName] = startupFileName;
            buildProperties[DotnetCoreConstants.PublishDir] = publishDir;
            buildProperties[DotnetCoreConstants.ProjectFile] = projectFile;

            bool zipAllOutput = ShouldZipAllOutput(context);
            buildProperties[ManifestFilePropertyKeys.ZipAllOutput] = zipAllOutput.ToString().ToLowerInvariant();

            var props = new DotNetCoreBashBuildSnippetProperties
            {
                ProjectFile = projectFile,
                PublishDirectory = publishDir,
                ZipAllOutput = zipAllOutput
            };
            string script = TemplateHelpers.Render(TemplateHelpers.TemplateResource.DotNetCoreSnippet, props, _logger);
            return new BuildScriptSnippet { BashBuildScriptSnippet = script, BuildProperties = buildProperties };
        }

        public bool IsCleanRepo(ISourceRepo repo)
        {
            (_, string expectedPublishDir) = GetProjectFileAndPublishDir(repo);
            return !repo.DirExists(expectedPublishDir);
        }

        public string GenerateBashRunScript(RunScriptGeneratorOptions runScriptGeneratorOptions)
        {
            throw new System.NotImplementedException();
        }

        public bool IsEnabled(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return scriptGeneratorContext.EnableDotNetCore;
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
            context.DotnetCoreVersion = version;
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
            dirs.Add("obj");
            dirs.Add("bin");
            dirs.Add(DotnetCoreConstants.OryxOutputPublishDirectory);
            return dirs;
        }

        private static bool ShouldZipAllOutput(BuildScriptGeneratorContext context)
        {
            return BuildPropertiesHelper.IsTrue(
                Constants.ZipAllOutputBuildPropertyKey,
                context,
                valueIsRequired: false);
        }

        private (string projFile, string publishDir) GetProjectFileAndPublishDir(ISourceRepo repo)
        {
            var projectFile = _aspNetCoreWebAppProjectFileProvider.GetProjectFile(repo);
            if (string.IsNullOrEmpty(projectFile))
            {
                return (null, null);
            }

            var publishDir = Path.Combine(
                new FileInfo(projectFile).Directory.FullName,
                DotnetCoreConstants.OryxOutputPublishDirectory);
            return (projectFile, publishDir);
        }
    }
}