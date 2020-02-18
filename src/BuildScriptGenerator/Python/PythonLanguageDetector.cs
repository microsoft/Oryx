// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonLanguageDetector : ILanguageDetector
    {
        private readonly IPythonVersionProvider _versionProvider;
        private readonly ILogger<PythonLanguageDetector> _logger;

        public PythonLanguageDetector(
            IPythonVersionProvider pythonVersionProvider,
            ILogger<PythonLanguageDetector> logger,
            IStandardOutputWriter writer)
        {
            _versionProvider = pythonVersionProvider;
            _logger = logger;
        }

        public LanguageDetectorResult Detect(RepositoryContext context)
        {
            var sourceRepo = context.SourceRepo;
            if (!sourceRepo.FileExists(PythonConstants.RequirementsFileName))
            {
                _logger.LogDebug($"File '{PythonConstants.RequirementsFileName}' does not exist in source repo");
                return null;
            }

            string runtimeVersion = DetectPythonVersionFromRuntimeFile(sourceRepo);

            if (string.IsNullOrEmpty(runtimeVersion))
            {
                var files = sourceRepo.EnumerateFiles(
                    PythonConstants.PythonFileNamePattern,
                    searchSubDirectories: false);

                if (files == null || !files.Any())
                {
                    _logger.LogDebug($"Files with extension '{PythonConstants.PythonFileNamePattern}' do not exist " +
                        "in source repo root");
                    return null;
                }
            }

            runtimeVersion = VerifyAndResolveVersion(runtimeVersion);

            return new LanguageDetectorResult
            {
                Language = PythonConstants.PythonName,
                LanguageVersion = runtimeVersion,
            };
        }

        private string VerifyAndResolveVersion(string version)
        {
            // Get the versions either from disk or on the web
            var versionInfo = _versionProvider.GetVersionInfo();

            // Get the default version. This could be having just the major or major.minor version.
            // So try getting the latest version of the default version.
            if (string.IsNullOrEmpty(version))
            {
                version = versionInfo.DefaultVersion;
            }

            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                version,
                versionInfo.SupportedVersions);

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exc = new UnsupportedVersionException(
                    PythonConstants.PythonName,
                    version,
                    versionInfo.SupportedVersions);
                _logger.LogError(
                    exc,
                    $"Exception caught, the version '{version}' is not supported for the Python platform.");
                throw exc;
            }

            return maxSatisfyingVersion;
        }

        private string DetectPythonVersionFromRuntimeFile(ISourceRepo sourceRepo)
        {
            const string versionPrefix = "python-";

            // Most Python sites will have at least a .py file in the root, but
            // some may not. In that case, let them opt in with the runtime.txt
            // file, which is used to specify the version of Python.
            if (sourceRepo.FileExists(PythonConstants.RuntimeFileName))
            {
                try
                {
                    var content = sourceRepo.ReadFile(PythonConstants.RuntimeFileName);
                    var hasPythonVersion = content.StartsWith(versionPrefix, StringComparison.OrdinalIgnoreCase);
                    if (!hasPythonVersion)
                    {
                        _logger.LogDebug(
                            "Prefix {verPrefix} was not found in file {rtFileName}",
                            versionPrefix,
                            PythonConstants.RuntimeFileName.Hash());
                        return null;
                    }

                    var pythonVersion = content.Remove(0, versionPrefix.Length);
                    _logger.LogDebug("Found version {pyVer} in runtime file", pythonVersion);
                    return pythonVersion;
                }
                catch (IOException ex)
                {
                    _logger.LogError(
                        ex,
                        "An error occurred while reading file {rtFileName}",
                        PythonConstants.RuntimeFileName.Hash());
                }
            }
            else
            {
                _logger.LogDebug(
                    "Could not find file '{rtFileName}' in source repo",
                    PythonConstants.RuntimeFileName);
            }

            return null;
        }
    }
}