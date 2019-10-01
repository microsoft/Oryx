// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class ProbeAndFindProjectFileProvider : IProjectFileProvider
    {
        private readonly ILogger<ProbeAndFindProjectFileProvider> _logger;
        private readonly IStandardOutputWriter _writer;

        // Since this service is registered as a singleton, we can cache the lookup of project file.
        private bool _probedForProjectFile;
        private string _projectFileRelativePath;

        public ProbeAndFindProjectFileProvider(ILogger<ProbeAndFindProjectFileProvider> logger, IStandardOutputWriter writer)
        {
            _logger = logger;
            _writer = writer;
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
            // to websdk or azure functions

            // Since enumerating all files in the directory may take some time, write a message using the
            // given IStandardOutputWriter to alert the user of what is happening.
            _writer.WriteLine(string.Format(Constants.EnumerateFilesInRepo, DotNetCoreConstants.CSharpProjectFileExtension));

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

            var webAppProjects = new List<string>();
            var azureFunctionsProjects = new List<string>();
            var allProjects = new List<string>();
            foreach (var file in projectFiles)
            {
                allProjects.Add(file);
                if (ProjectFileHelpers.IsAspNetCoreWebApplicationProject(sourceRepo, file))
                {
                    webAppProjects.Add(file);
                }
                else if (ProjectFileHelpers.IsAzureFunctionsProject(sourceRepo, file))
                {
                    azureFunctionsProjects.Add(file);
                }
            }

            projectFile = GetProject(webAppProjects);
            if (projectFile == null)
            {
                projectFile = GetProject(azureFunctionsProjects);
            }

            if (projectFile == null)
            {
                _logger.LogDebug(
                    $"Could not find a project file to build. Available project files: " +
                    $"{string.Join(',', allProjects)}");
                return null;
            }

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

        private string GetProject(List<string> projects)
        {
            if (projects.Count > 1)
            {
                var projectList = string.Join(", ", projects);
                throw new InvalidUsageException(string.Format(
                    Resources.Labels.DotNetCoreAmbiguityInSelectingProjectFile,
                    projectList,
                    EnvironmentSettingsKeys.Project));
            }

            if (projects.Count == 1)
            {
                return projects[0];
            }

            return null;
        }
    }
}
