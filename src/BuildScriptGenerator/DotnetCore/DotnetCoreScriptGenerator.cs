// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
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
        private readonly IEnvironmentSettingsProvider _environmentSettingsProvider;

        public DotnetCoreScriptGenerator(
            IOptions<DotnetCoreScriptGeneratorOptions> scriptGeneratorOptions,
            IDotnetCoreVersionProvider versionProvider,
            IEnvironmentSettingsProvider environmentSettingsProvider,
            ILogger<DotnetCoreScriptGenerator> logger)
        {
            _scriptGeneratorOptions = scriptGeneratorOptions.Value;
            _versionProvider = versionProvider;
            _environmentSettingsProvider = environmentSettingsProvider;
            _logger = logger;
        }

        public string SupportedLanguageName => DotnetCoreConstants.LanguageName;

        public IEnumerable<string> SupportedLanguageVersions => _versionProvider.SupportedVersions;

        public bool TryGenerateBashScript(ScriptGeneratorContext scriptGeneratorContext, out string script)
        {
            _environmentSettingsProvider.TryGetAndLoadSettings(out var environmentSettings);

            script = new DotnetCoreBashBuildScript(
                preBuildScriptPath: environmentSettings?.PreBuildScriptPath,
                benvArgs: $"dotnet={scriptGeneratorContext.LanguageVersion}",
                publishDirectory: DotnetCoreConstants.OryxOutputPublishDirectory,
                postBuildScriptPath: environmentSettings?.PostBuildScriptPath).TransformText();

            return true;
        }
    }
}
