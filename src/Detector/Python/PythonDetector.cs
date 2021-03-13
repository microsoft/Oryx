// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common.Extensions;
using Microsoft.Oryx.Detector.Exceptions;
using YamlDotNet.RepresentationModel;

namespace Microsoft.Oryx.Detector.Python
{
    /// <summary>
    /// An implementation of <see cref="IPlatformDetector"/> which detects Python applications.
    /// </summary>
    public class PythonDetector : IPythonPlatformDetector
    {
        private readonly ILogger<PythonDetector> _logger;
        private readonly DetectorOptions _options;

        /// <summary>
        /// Creates an instance of <see cref="PythonDetector"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{PythonDetector}"/>.</param>
        /// <param name="options">The <see cref="DetectorOptions"/>.</param>
        public PythonDetector(ILogger<PythonDetector> logger, IOptions<DetectorOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        /// <inheritdoc/>
        public PlatformDetectorResult Detect(DetectorContext context)
        {
            var sourceRepo = context.SourceRepo;
            var appDirectory = string.Empty;
            var hasRequirementsTxtFile = false;
            var hasPyprojectTomlFile = false;
            if (sourceRepo.FileExists(PythonConstants.RequirementsFileName))
            {
                _logger.LogInformation($"Found {PythonConstants.RequirementsFileName} at the root of the repo.");
                hasRequirementsTxtFile = true;
            }
            else
            {
                _logger.LogInformation(
                    $"Cound not find {PythonConstants.RequirementsFileName} at the root of the repo.");
            }
            if (sourceRepo.FileExists(PythonConstants.PyprojectTomlFileName))
            {
                _logger.LogInformation($"Found {PythonConstants.PyprojectTomlFileName} at the root of the repo.");
                hasPyprojectTomlFile = true;
            }
            var hasCondaEnvironmentYmlFile = false;
            if (sourceRepo.FileExists(PythonConstants.CondaEnvironmentYmlFileName) &&
                IsCondaEnvironmentFile(sourceRepo, PythonConstants.CondaEnvironmentYmlFileName))
            {
                _logger.LogInformation(
                    $"Found {PythonConstants.CondaEnvironmentYmlFileName} at the root of the repo.");
                hasCondaEnvironmentYmlFile = true;
            }

            if (!hasCondaEnvironmentYmlFile &&
                sourceRepo.FileExists(PythonConstants.CondaEnvironmentYamlFileName) &&
                IsCondaEnvironmentFile(sourceRepo, PythonConstants.CondaEnvironmentYamlFileName))
            {
                _logger.LogInformation(
                    $"Found {PythonConstants.CondaEnvironmentYamlFileName} at the root of the repo.");
                hasCondaEnvironmentYmlFile = true;
            }

            var hasJupyterNotebookFiles = false;
            var notebookFiles = sourceRepo.EnumerateFiles(
                $"*.{PythonConstants.JupyterNotebookFileExtensionName}",
                searchSubDirectories: false);
            if (notebookFiles != null && notebookFiles.Any())
            {
                _logger.LogInformation(
                    $"Found files with extension {PythonConstants.JupyterNotebookFileExtensionName} " +
                    $"at the root of the repo.");
                hasJupyterNotebookFiles = true;
            }

            // This detects if a runtime.txt file exists and if that is a python file
            var hasRuntimeTxtFile = false;
            var versionFromRuntimeFile = DetectPythonVersionFromRuntimeFile(context.SourceRepo);
            if (!string.IsNullOrEmpty(versionFromRuntimeFile))
            {
                hasRuntimeTxtFile = true;
            }

            if (!hasRequirementsTxtFile &&
                !hasCondaEnvironmentYmlFile &&
                !hasJupyterNotebookFiles &&
                !hasRuntimeTxtFile &&
                !hasPyprojectTomlFile)
            {
                var searchSubDirectories = !_options.DisableRecursiveLookUp;
                if (!searchSubDirectories)
                {
                    _logger.LogDebug("Skipping search for files in sub-directories as it has been disabled.");
                }

                var files = sourceRepo.EnumerateFiles(PythonConstants.PythonFileNamePattern, searchSubDirectories);
                if (files != null && files.Any())
                {
                    _logger.LogInformation(
                        $"Found files with extension '{PythonConstants.PythonFileNamePattern}' " +
                        $"in the repo.");
                    appDirectory = RelativeDirectoryHelper.GetRelativeDirectoryToRoot(
                        files.FirstOrDefault(), sourceRepo.RootPath);
                }
                else
                {
                    _logger.LogInformation(
                        $"Could not find any file with extension '{PythonConstants.PythonFileNamePattern}' " +
                        $"in the repo.");
                    return null;
                }
            }

            return new PythonPlatformDetectorResult
            {
                Platform = PythonConstants.PlatformName,
                PlatformVersion = versionFromRuntimeFile,
                AppDirectory = appDirectory,
                HasJupyterNotebookFiles = hasJupyterNotebookFiles,
                HasCondaEnvironmentYmlFile = hasCondaEnvironmentYmlFile,
                HasRequirementsTxtFile = hasRequirementsTxtFile,
                HasPyprojectTomlFile = hasPyprojectTomlFile,
            };
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
                    _logger.LogDebug(
                        "Found version {pyVer} in the {rtFileName} file",
                        pythonVersion,
                        PythonConstants.RuntimeFileName.Hash());
                    return pythonVersion;
                }
                catch (IOException ex)
                {
                    _logger.LogError(
                        ex,
                        "An error occurred while reading file {rtFileName}",
                        PythonConstants.RuntimeFileName);
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

        private bool IsCondaEnvironmentFile(ISourceRepo sourceRepo, string fileName)
        {
            YamlNode yamlNode = null;

            try
            {
                yamlNode = ParserHelper.ParseYamlFile(sourceRepo, fileName);
            }
            catch (FailedToParseFileException ex)
            {
                _logger.LogError(ex, $"An error occurred when trying to parse file '{fileName}'.");
                return false;
            }

            var yamlMappingNode = yamlNode as YamlMappingNode;
            if (yamlMappingNode != null)
            {
                if (yamlMappingNode.Children.Keys
                    .Select(key => key.ToString())
                    .Any(key => PythonConstants.CondaEnvironmentFileKeys.Contains(
                        key,
                        StringComparer.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}