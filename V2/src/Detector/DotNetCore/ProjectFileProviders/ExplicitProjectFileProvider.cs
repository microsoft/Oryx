// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector.DotNetCore
{
    /// <summary>
    /// Gets the relative path to the project file which user requested explicitly through the 'PROJECT'
    /// environment variable.
    /// </summary>
    internal class ExplicitProjectFileProvider : IProjectFileProvider
    {
        private readonly IOptions<DetectorOptions> options;
        private readonly ILogger<ExplicitProjectFileProvider> logger;

        public ExplicitProjectFileProvider(
            IOptions<DetectorOptions> options,
            ILogger<ExplicitProjectFileProvider> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public string GetRelativePathToProjectFile(DetectorContext context)
        {
            var projectPath = this.GetProjectInfoFromSettings();
            if (string.IsNullOrEmpty(projectPath))
            {
                this.logger.LogDebug(
                    "No request to build a particular project file explicitly using the " +
                    $"PROJECT environment variable");
                return null;
            }

            var projectFileWithRelativePath = projectPath.Trim();
            var projectFile = Path.Combine(context.SourceRepo.RootPath, projectFileWithRelativePath);
            if (context.SourceRepo.FileExists(projectFile))
            {
                this.logger.LogDebug($"Using the given .NET Core project file to build.");
            }
            else
            {
                this.logger.LogWarning($"Could not find the .NET Core project file.");
                throw new InvalidProjectFileException("Could not find the .NET Core project file.");
            }

            return projectFileWithRelativePath;
        }

        private string GetProjectInfoFromSettings()
        {
            if (!string.IsNullOrEmpty(this.options.Value.Project))
            {
                return this.options.Value.Project;
            }

            return null;
        }
    }
}
