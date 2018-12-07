// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonLanguageDetector : ILanguageDetector
    {
        private readonly PythonScriptGeneratorOptions _pythonScriptGeneratorOptions;
        private readonly IPythonVersionProvider _pythonVersionProvider;
        private readonly ILogger<PythonLanguageDetector> _logger;

        public PythonLanguageDetector(
            IOptions<PythonScriptGeneratorOptions> options,
            IPythonVersionProvider pythonVersionProvider,
            ILogger<PythonLanguageDetector> logger)
        {
            _pythonScriptGeneratorOptions = options.Value;
            _pythonVersionProvider = pythonVersionProvider;
            _logger = logger;
        }

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            if (!sourceRepo.FileExists(Constants.RequirementsFileName))
            {
                _logger.LogDebug("File {ReqsFileName} does not exist in source repo", Constants.RequirementsFileName);
                return null;
            }

            string runtimeVersion = DetectPythonVersionFromRuntimeFile(sourceRepo);

            if (string.IsNullOrEmpty(runtimeVersion))
            {
                var files = sourceRepo.EnumerateFiles(Constants.PythonFileExtension, searchSubDirectories: false);
                if (files == null || !files.Any())
                {
                    _logger.LogDebug($"Could not find any files with extension '{Constants.PythonFileExtension}' in source repo");
                    return null;
                }
            }

            runtimeVersion = VerifyAndResolveVersion(runtimeVersion);

            return new LanguageDetectorResult
            {
                Language = Constants.PythonName,
                LanguageVersion = runtimeVersion,
            };
        }

        private string VerifyAndResolveVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return _pythonScriptGeneratorOptions.PythonDefaultVersion;
            }
            else
            {
                var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                    version,
                    _pythonVersionProvider.SupportedPythonVersions);

                if (string.IsNullOrEmpty(maxSatisfyingVersion))
                {
                    var exc = new UnsupportedVersionException($"Target Python version '{version}' is unsupported. " +
                        $"Supported versions are: {string.Join(", ", _pythonVersionProvider.SupportedPythonVersions)}");
                    _logger.LogError(exc, "Exception caught");
                    throw exc;
                }

                return maxSatisfyingVersion;
            }
        }

        private string DetectPythonVersionFromRuntimeFile(ISourceRepo sourceRepo)
        {
            const string versionPrefix = "python-";

            // Most Python sites will have at least a .py file in the root, but
            // some may not. In that case, let them opt in with the runtime.txt
            // file, which is used to specify the version of Python.
            if (sourceRepo.FileExists(Constants.RuntimeFileName))
            {
                try
                {
                    var content = sourceRepo.ReadFile(Constants.RuntimeFileName);
                    var hasPythonVersion = content.StartsWith(versionPrefix, StringComparison.OrdinalIgnoreCase);
                    if (!hasPythonVersion)
                    {
                        _logger.LogDebug("Prefix {VerPrefix} was not found in file {RtFileName}", versionPrefix, Constants.RuntimeFileName);
                        return null;
                    }

                    var pythonVersion = content.Remove(0, versionPrefix.Length);

                    _logger.LogDebug("Found version {PyVer} in file {RtFileName}", pythonVersion, Constants.RuntimeFileName);

                    return pythonVersion;
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "An error occurred while reading file {RtFileName}", Constants.RuntimeFileName);
                }
            }
            else
            {
                _logger.LogDebug("Could not find file '{RtFileName}' in source repo", Constants.RuntimeFileName);
            }

            return null;
        }
    }
}
