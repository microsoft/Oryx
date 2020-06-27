// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
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

        public string GetRelativePathToProjectFile(RepositoryContext context)
        {
            if (_probedForProjectFile)
            {
                return _projectFileRelativePath;
            }

            var sourceRepo = context.SourceRepo;
            string projectFile = null;

            // Check if any of the sub-directories has a .csproj or .fsproj file and if that file has references
            // to websdk or azure functions

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
            var azureBlazorWasmProjects = new List<string>();
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
                else if (ProjectFileHelpers.IsAzureBlazorWebAssemblyProject(sourceRepo, file))
                {
                    azureBlazorWasmProjects.Add(file);
                }
            }

            // Assumption: some env variable "Oryx_App_type" will be passed to oryx
            // which will indicate if the app is an azure function type or static type app
            var oryxAppTypeEnvironmentVar = Environment.GetEnvironmentVariable("Oryx_App_Type");
            if (projectFile == null && !string.IsNullOrEmpty(oryxAppTypeEnvironmentVar)
                && oryxAppTypeEnvironmentVar.ToLower().Contains("functions"))
            {
                projectFile = GetProject(azureFunctionsProjects);
            }
            else if (projectFile == null && !string.IsNullOrEmpty(oryxAppTypeEnvironmentVar)
               && (oryxAppTypeEnvironmentVar.ToLower().Contains("static-sites") || oryxAppTypeEnvironmentVar.ToLower().Contains("blazor-wasm")))
            {
                projectFile = GetProject(azureBlazorWasmProjects);
            }
            else if (projectFile == null)
            {
                // Not sure if vanilla web-project will be denoted by setting any value in Oryx_DotNetCore_App_Type
                // so assuming it will be null for vanilla dotnet core web app
                projectFile = GetProject(webAppProjects);
            }
            
            // After scanning all the project types we stil didn't find any files (e.g. csproj
            if (projectFile == null)
            {
                _logger.LogDebug("Could not find a .NET Core project file to build.");
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
