// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotnetCore
{
    public class DotnetCoreScriptGenerator : ILanguageScriptGenerator
    {
        private readonly DotnetCoreScriptGeneratorOptions _scriptGeneratorOptions;
        private readonly IDotnetCoreVersionProvider _versionProvider;
        private readonly IAspNetCoreWebAppProjectFileProvider _aspNetCoreWebAppProjectFileProvider;
        private readonly ILogger<DotnetCoreScriptGenerator> _logger;

        public DotnetCoreScriptGenerator(
            IOptions<DotnetCoreScriptGeneratorOptions> scriptGeneratorOptions,
            IDotnetCoreVersionProvider versionProvider,
            IAspNetCoreWebAppProjectFileProvider aspNetCoreWebAppProjectFileProvider,
            ILogger<DotnetCoreScriptGenerator> logger)
        {
            _scriptGeneratorOptions = scriptGeneratorOptions.Value;
            _versionProvider = versionProvider;
            _aspNetCoreWebAppProjectFileProvider = aspNetCoreWebAppProjectFileProvider;
            _logger = logger;
        }

        public string SupportedLanguageName => DotnetCoreConstants.LanguageName;

        public IEnumerable<string> SupportedLanguageVersions => _versionProvider.SupportedVersions;

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
                BashBuildScriptSnippet = script,
                RequiredToolsVersion = new Dictionary<string, string>()
                {
                    { "dotnet", scriptGeneratorContext.LanguageVersion }
                }
            };
        }
    }
}