// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpLanguageDetector : ILanguageDetector
    {
        private readonly PhpScriptGeneratorOptions _opts;
        private readonly IPhpVersionProvider _versionProvider;
        private readonly ILogger<PhpLanguageDetector> _logger;

        public PhpLanguageDetector(
            IOptions<PhpScriptGeneratorOptions> options,
            IPhpVersionProvider versionProvider,
            ILogger<PhpLanguageDetector> logger)
        {
            _opts = options.Value;
            _versionProvider = versionProvider;
            _logger = logger;
        }

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            if (!sourceRepo.FileExists(PhpConstants.ComposerFileName))
            {
                _logger.LogDebug($"File '{PhpConstants.ComposerFileName}' does not exist in source repo");
                return null;
            }

            dynamic composerFile = SourceRepo.SourceRepoFileHelpers.ReadJsonObjectFromFile(sourceRepo, PhpConstants.ComposerFileName);
            string runtimeVersion = ResolveVersionFromComposerSpec(composerFile?.require?.php) ?? PhpConstants.DefaultPhpRuntimeVersion;
            return new LanguageDetectorResult
            {
                Language = PhpConstants.PhpName,
                LanguageVersion = runtimeVersion,
            };
        }

        /// <summary>
        /// Resolve a version specication string, like "^5.5 || ^7.0", to a single selection from the available versions.
        /// </summary>
        /// <param name="spec">`composer.json` version specification string</param>
        /// <returns>Resolved PHP runtime version.</returns>
        private string ResolveVersionFromComposerSpec([CanBeNull] string spec)
        {
            return null;
        }
    }
}
