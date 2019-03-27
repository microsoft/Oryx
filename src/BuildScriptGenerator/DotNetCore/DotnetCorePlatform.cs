// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// .NET Core platform.
    /// </summary>
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

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            (string projectFile, string publishDir) = GetProjectFileAndPublishDir(scriptGeneratorContext.SourceRepo);
            if (string.IsNullOrEmpty(projectFile) || string.IsNullOrEmpty(publishDir))
            {
                return null;
            }

            var props = new DotNetCoreBashBuildSnippetProperties
            {
                ProjectFile = projectFile,
                PublishDirectory = publishDir
            };
            string script = TemplateHelpers.Render(TemplateHelpers.TemplateResource.DotNetCoreSnippet, props, _logger);
            return new BuildScriptSnippet { BashBuildScriptSnippet = script };
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
            return Array.Empty<string>();
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return Array.Empty<string>();
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