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
    class ExplicitProjectFileProvider : IProjectFileProvider
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
                return null;
            }

            var projectFileWithRelativePath = projectPath.Trim();
            var projectFile = Path.Combine(context.SourceRepo.RootPath, projectFileWithRelativePath);
            if (!context.SourceRepo.FileExists(projectFile))
            {
                _logger.LogWarning($"Could not find the project file '{projectFile}'.");
                throw new InvalidUsageException(
                    $"Could not find the project file '{projectFile}' specified by the environment variable" +
                    $" '{EnvironmentSettingsKeys.Project}' with value '{projectFileWithRelativePath}'. " +
                    "Make sure the path to the project file is relative to the root of the repo. " +
                    "For example: PROJECT=src/Dashboard/Dashboard.csproj");
            }

            return projectFileWithRelativePath;
        }

        private string GetProjectInfoFromSettings(BuildScriptGeneratorContext context)
        {
            // Value from command line has higher precedence than from environment variables

            string project = null;
            if (context.Properties != null && context.Properties.TryGetValue(
                DotNetCoreConstants.ProjectBuildPropertyKey,
                out var projectFromProperty))
            {
                project = projectFromProperty;
            }
            else if (!string.IsNullOrEmpty(_options.Project))
            {
                project = _options.Project;
            }

            return project;
        }
    }
}
