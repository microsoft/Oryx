// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.Detector.DotNetCore
{
    /// <summary>
    /// Gets the relative path to the project file which user requested explicitly through either the 'PROJECT'
    /// environment variable or the 'project' build property.
    /// </summary>
    public class ExplicitProjectFileProvider : IProjectFileProvider
    {
        private readonly IOptions<DetectorOptions> _options;
        private readonly ILogger<ExplicitProjectFileProvider> _logger;

        public ExplicitProjectFileProvider(
            IOptions<DetectorOptions> options,
            ILogger<ExplicitProjectFileProvider> logger)
        {
            _options = options;
            _logger = logger;
        }

        public string GetRelativePathToProjectFile(RepositoryContext context)
        {
            var projectPath = GetProjectInfoFromSettings(context);
            if (string.IsNullOrEmpty(projectPath))
            {
                _logger.LogDebug(
                    "No request to build a particular project file explicitly either using the " +
                    $"PROJECT environment variable or the " +
                    $"'{DotNetCoreConstants.ProjectBuildPropertyKey}' build property");
                return null;
            }

            var projectFileWithRelativePath = projectPath.Trim();
            var projectFile = Path.Combine(context.SourceRepo.RootPath, projectFileWithRelativePath);
            if (context.SourceRepo.FileExists(projectFile))
            {
                _logger.LogDebug($"Using the given .NET Core project file to build.");
            }
            else
            {
                _logger.LogWarning($"Could not find the .NET Core project file.");
                throw new InvalidProjectFileException("Could not find the .NET Core project file.");
            }

            return projectFileWithRelativePath;
        }

        private string GetProjectInfoFromSettings(RepositoryContext context)
        {
            // Value from command line has higher precedence than from environment variables
            if (context.Properties != null && context.Properties.TryGetValue(
                DotNetCoreConstants.ProjectBuildPropertyKey,
                out var projectFromProperty))
            {
                return projectFromProperty;
            }

            if (!string.IsNullOrEmpty(_options.Value.Project))
            {
                return _options.Value.Project;
            }

            return null;
        }
    }
}
