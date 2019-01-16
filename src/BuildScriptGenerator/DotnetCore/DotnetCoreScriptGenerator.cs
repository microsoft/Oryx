// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotnetCore
{
    public class DotnetCoreScriptGenerator : ILanguageScriptGenerator
    {
        private readonly DotnetCoreScriptGeneratorOptions _scriptGeneratorOptions;
        private readonly IDotnetCoreVersionProvider _versionProvider;
        private readonly ILogger<DotnetCoreScriptGenerator> _logger;

        public DotnetCoreScriptGenerator(
            IOptions<DotnetCoreScriptGeneratorOptions> scriptGeneratorOptions,
            IDotnetCoreVersionProvider versionProvider,
            ILogger<DotnetCoreScriptGenerator> logger)
        {
            _scriptGeneratorOptions = scriptGeneratorOptions.Value;
            _versionProvider = versionProvider;
            _logger = logger;
        }

        public string SupportedLanguageName => DotnetCoreConstants.LanguageName;

        public IEnumerable<string> SupportedLanguageVersions => _versionProvider.SupportedVersions;

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(ScriptGeneratorContext scriptGeneratorContext)
        {
            var script = new DotnetCoreBashBuildSnippet(
                publishDirectory: DotnetCoreConstants.OryxOutputPublishDirectory).TransformText();

            return new BuildScriptSnippet()
            {
                BashBuildScriptSnippet = script,
                RequiredToolsVersion = new Dictionary<string, string>() { { "dotnet", scriptGeneratorContext.LanguageVersion } }
            };
        }
    }
}