// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.DotnetCore
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

        public IEnumerable<string> SupportedLanguageVersions => _versionProvider.SupportedVersions;

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            return _detector.Detect(sourceRepo);
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(ScriptGeneratorContext scriptGeneratorContext)
        {
            var projectFile = _aspNetCoreWebAppProjectFileProvider.GetProjectFile(scriptGeneratorContext.SourceRepo);
            if (string.IsNullOrEmpty(projectFile))
            {
                return null;
            }

            var projectDir = new FileInfo(projectFile).Directory.FullName;
            var publishDir = Path.Combine(projectDir, DotnetCoreConstants.OryxOutputPublishDirectory);
            var script = new DotnetCoreBashBuildSnippet(
                publishDirectory: publishDir,
                projectFile: projectFile).TransformText();

            return new BuildScriptSnippet()
            {
                BashBuildScriptSnippet = script
            };
        }

        public bool IsEnabled(ScriptGeneratorContext scriptGeneratorContext)
        {
            return scriptGeneratorContext.EnableDotNetCore;
        }

        public void SetRequiredTools(ISourceRepo sourceRepo, string targetPlatformVersion, IDictionary<string, string> toolsToVersion)
        {
            Debug.Assert(toolsToVersion != null, $"{nameof(toolsToVersion)} must not be null.");
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion["dotnet"] = targetPlatformVersion;
            }
        }

        public void SetVersion(ScriptGeneratorContext context, string version)
        {
            context.DotnetCoreVersion = version;
        }
    }
}