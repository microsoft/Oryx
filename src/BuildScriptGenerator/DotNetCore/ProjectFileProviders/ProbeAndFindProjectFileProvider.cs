// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class ProbeAndFindProjectFileProvider : IProjectFileProvider
    {
        private readonly ILogger<ProbeAndFindProjectFileProvider> _logger;

        // Since this service is registered as a singleton, we can cache the lookup of project file.
        private bool _probedForProjectFile;
        private string _projectFileRelativePath;

        public ProbeAndFindProjectFileProvider(ILogger<ProbeAndFindProjectFileProvider> logger)
        {
            _logger = logger;
        }

        public string GetRelativePathToProjectFile(BuildScriptGeneratorContext context)
        {
            if (_probedForProjectFile)
            {
                return _projectFileRelativePath;
            }

            var sourceRepo = context.SourceRepo;
            string projectFile = null;

            // Check if any of the sub-directories has a .csproj or .fsproj file and if that file has references
            // websdk

            // search for .csproj files
            var projectFiles = GetAllProjectFilesInRepo(
                    sourceRepo,
                    DotNetCoreConstants.CSharpProjectFileExtension);

            if (!projectFiles.Any())
            {
                _logger.LogDebug(
                    "Could not find any files with extension " +
                    $"'{DotNetCoreConstants.CSharpProjectFileExtension}' in repo.");

                // search for .fsproj files
                projectFiles = GetAllProjectFilesInRepo(
                    sourceRepo,
                    DotNetCoreConstants.FSharpProjectFileExtension);

                if (!projectFiles.Any())
                {
                    _logger.LogDebug(
                        "Could not find any files with extension " +
                        $"'{DotNetCoreConstants.FSharpProjectFileExtension}' in repo.");
                    return null;
                }
            }

            var projects = new List<string>();
            foreach (var file in projectFiles)
            {
                if (ProjectFileHelpers.IsAspNetCoreWebApplicationProject(sourceRepo, file))
                {
                    projects.Add(file);
                }
                else if (ProjectFileHelpers.IsAzureFunctionsProject(sourceRepo, file))
                {
                    projects.Add(file);
                }
            }

            if (projects.Count > 1)
            {
                var projectList = string.Join(", ", projects);
                throw new InvalidUsageException(
                    "Ambiguity in selecting a project to build. " +
                    $"Found multiple projects: '{projectList}'. To fix this, use the environment variable " +
                    $"'{EnvironmentSettingsKeys.Project}' to specify the relative path to the project " +
                    "to be deployed.");
            }

            if (projects.Count == 0)
            {
                var projectList = string.Join(", ", projects);
                _logger.LogDebug(
                    "Could not find any ASP.NET Core web application or Azure projects to build. " +
                    $"Found the following project files: '{projectList}'. " +
                    $"To fix this, use the environment variable '{EnvironmentSettingsKeys.Project}' to specify the " +
                    "relative path to the project to be deployed.");
                return null;
            }

            projectFile = projects[0];

            // Cache the results
            _probedForProjectFile = true;
            _projectFileRelativePath = ProjectFileHelpers.GetRelativePathToRoot(projectFile, sourceRepo.RootPath);
            return _projectFileRelativePath;
        }

        private static IEnumerable<string> GetAllProjectFilesInRepo(
            ISourceRepo sourceRepo,
            string projectFileExtension)
        {
            return sourceRepo.EnumerateFiles($"*.{projectFileExtension}", searchSubDirectories: true);
        }
    }
}
