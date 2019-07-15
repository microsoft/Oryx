// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// Gets the relative path to the project file which user requested explicitly through either the 'PROJECT'
    /// environment variable or the 'project' build property.
    /// </summary>
    internal class ExplicitProjectFileProvider : IProjectFileProvider
    {
        private readonly DotNetCoreScriptGeneratorOptions _options;
        private readonly ILogger<ExplicitProjectFileProvider> _logger;

        public ExplicitProjectFileProvider(
            IOptions<DotNetCoreScriptGeneratorOptions> options,
            ILogger<ExplicitProjectFileProvider> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public string GetRelativePathToProjectFile(BuildScriptGeneratorContext context)
        {
            var projectPath = GetProjectInfoFromSettings(context);
            if (string.IsNullOrEmpty(projectPath))
            {
                _logger.LogDebug(
                    "No request to build a particular project file explicitly either using the " +
                    $"'{EnvironmentSettingsKeys.Project}' environment variable or the " +
                    $"'{DotNetCoreConstants.ProjectBuildPropertyKey}' build property");
                return null;
            }

            var projectFileWithRelativePath = projectPath.Trim();
            var projectFile = Path.Combine(context.SourceRepo.RootPath, projectFileWithRelativePath);
            if (context.SourceRepo.FileExists(projectFile))
            {
                _logger.LogDebug($"Using the project file '{projectFile}' to build.");
            }
            else
            {
                _logger.LogWarning($"Could not find the project file '{projectFile}'.");
                throw new InvalidUsageException(
                    $"Could not find the project file '{projectFile}' specified in settings. Make sure the path to " +
                    "the project file is relative to the root of the repo. " +
                    "For example: PROJECT=src/Dashboard/Dashboard.csproj");
            }

            return projectFileWithRelativePath;
        }

        private string GetProjectInfoFromSettings(BuildScriptGeneratorContext context)
        {
            // Value from command line has higher precedence than from environment variables
            if (context.Properties != null && context.Properties.TryGetValue(
                DotNetCoreConstants.ProjectBuildPropertyKey,
                out var projectFromProperty))
            {
                return projectFromProperty;
            }

            if (!string.IsNullOrEmpty(_options.Project))
            {
                return _options.Project;
            }

            return null;
        }
    }
}
