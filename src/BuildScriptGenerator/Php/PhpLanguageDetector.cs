// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.SourceRepo;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpLanguageDetector : ILanguageDetector
    {
        private readonly PhpScriptGeneratorOptions _opts;
        private readonly IPhpVersionProvider _versionProvider;
        private readonly ILogger<PhpLanguageDetector> _logger;
        private readonly IStandardOutputWriter _writer;

        public PhpLanguageDetector(
            IOptions<PhpScriptGeneratorOptions> options,
            IPhpVersionProvider versionProvider,
            ILogger<PhpLanguageDetector> logger,
            IStandardOutputWriter writer)
        {
            _opts = options.Value;
            _versionProvider = versionProvider;
            _logger = logger;
            _writer = writer;
        }

        public LanguageDetectorResult Detect(RepositoryContext context)
        {
            var sourceRepo = context.SourceRepo;
            if (!sourceRepo.FileExists(PhpConstants.ComposerFileName))
            {
                _logger.LogDebug($"File '{PhpConstants.ComposerFileName}' does not exist in source repo");
                return null;
            }

            dynamic composerFile = null;
            try
            {
                composerFile = sourceRepo.ReadJsonObjectFromFile(PhpConstants.ComposerFileName);
            }
            catch (Exception ex)
            {
                // We just ignore errors, so we leave malformed composer.json files for Composer to handle,
                // not us. This prevents us from erroring out when Composer itself might be able to tolerate
                // some errors in the composer.json file.
                _logger.LogWarning(
                    ex,
                    $"Exception caught while trying to deserialize {PhpConstants.ComposerFileName.Hash()}");
            }

            string runtimeVersion = VerifyAndResolveVersion(composerFile?.require?.php?.Value as string);
            return new LanguageDetectorResult
            {
                Language = PhpConstants.PhpName,
                LanguageVersion = runtimeVersion,
            };
        }

        private string VerifyAndResolveVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return _opts.PhpDefaultVersion;
            }

            var matchingRange = SemanticVersionResolver.GetMatchingRange(
                version,
                _versionProvider.SupportedPhpVersions);
            if (!matchingRange.Equals(SemanticVersionResolver.NoRangeMatch))
            {
                return matchingRange.ToString();
            }

            _logger.LogError($"The version '{version}' is not supported for the PHP platform.");
            throw new UnsupportedVersionException(
                PhpConstants.PhpName,
                version,
                _versionProvider.SupportedPhpVersions);
        }
    }
}
