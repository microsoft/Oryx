using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpLanguageDetector : ILanguageDetector
    {
        private readonly PythonScriptGeneratorOptions _pythonScriptGeneratorOptions;
        private readonly IPythonVersionProvider _pythonVersionProvider;
        private readonly ILogger<PhpLanguageDetector> _logger;

        public PhpLanguageDetector(
            IOptions<PythonScriptGeneratorOptions> options,
            IPythonVersionProvider pythonVersionProvider,
            ILogger<PhpLanguageDetector> logger)
        {
            _pythonScriptGeneratorOptions = options.Value;
            _pythonVersionProvider = pythonVersionProvider;
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

            string runtimeVersionSpec = composerFile?.require?.php;
            string runtimeVersion = SelectVersionFromComposerSpec(composerFile?.require?.php) ?? PhpConstants.DefaultPhpRuntimeVersion;
            if (string.IsNullOrEmpty(runtimeVersionSpec))
            {
                
            }

            runtimeVersion = VerifyAndResolveVersion(runtimeVersion);

            return new LanguageDetectorResult
            {
                Language = PythonConstants.PythonName,
                LanguageVersion = runtimeVersion,
            };
        }

        private string SelectVersionFromComposerSpec([CanBeNull] string spec)
        {
            return null;
        }
    }
}
