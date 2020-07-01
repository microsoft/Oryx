// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class ProbeAndFindProjectFileProvider : IProjectFileProvider
    {
        private readonly ILogger<ProbeAndFindProjectFileProvider> _logger;
        private readonly IOptions<BuildScriptGeneratorOptions> _options;

        // Since this service is registered as a singleton, we can cache the lookup of project file.
        private bool _probedForProjectFile;
        private string _projectFileRelativePath;

        public ProbeAndFindProjectFileProvider(
            ILogger<ProbeAndFindProjectFileProvider> logger,
            IOptions<BuildScriptGeneratorOptions> options)
        {
            _logger = logger;
            _options = options;
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
            bool functionProjectExists = false;
            bool webAppProjectExists = false;
            bool blazorWasmProjectExists = false;

            foreach (var file in projectFiles)
            {
                allProjects.Add(file);
                if (ProjectFileHelpers.IsAzureBlazorWebAssemblyProject(sourceRepo, file))
                {
                    azureBlazorWasmProjects.Add(file);
                    blazorWasmProjectExists = true;
                }
                else if (ProjectFileHelpers.IsAzureFunctionsProject(sourceRepo, file))
                {
                    azureFunctionsProjects.Add(file);
                    functionProjectExists = true;
                }
                else if (ProjectFileHelpers.IsAspNetCoreWebApplicationProject(sourceRepo, file))
                {
                    webAppProjects.Add(file);
                    webAppProjectExists = true;
                }
            }

            // Assumption: some build option "--apptype" will be passed to oryx
            // which will indicate if the app is an azure function type or static type app

            // If there are multiple projects, we will look for --appty to detect corresponding project.
            // for example azurefunction and blazor both projects can reside
            // at the same repo, so more than 2 csprojs will be found. Now we will
            // look for --apptype value to determine which project needs to be built
            string oryxAppTypeEnv = string.Empty;

            if (_options != null
                && _options.Value != null
                && _options.Value.OryxAppType != null)
            {
                oryxAppTypeEnv = _options.Value.OryxAppType;
                _logger.LogInformation($"ProbeAndProjectFileProvider: {Constants.OryxAppType} is set to {oryxAppTypeEnv}");

                if (functionProjectExists && oryxAppTypeEnv.ToLower().Contains("functions"))
                {
                    projectFile = GetProject(azureFunctionsProjects);
                }
                else if (blazorWasmProjectExists
                    && (oryxAppTypeEnv.ToLower().Contains("static-sites")
                    || oryxAppTypeEnv.ToLower().Contains("blazor-wasm")))
                {
                    projectFile = GetProject(azureBlazorWasmProjects);
                }
                else
                {
                    _logger.LogDebug($"Invalid value {oryxAppTypeEnv} for env:{Constants.OryxAppType}. Currently, supported values are 'functions', 'blazor-wasm', 'static-sites'");
                }
            }
            else
            {
                // If multiple project exists, webapp gets priority if appType is not set
                // orderwise webapp then blazor-wasm and then functions
                if (projectFile == null && webAppProjectExists)
                {
                    projectFile = GetProject(webAppProjects);
                }
                else if (projectFile == null && blazorWasmProjectExists)
                {
                    projectFile = GetProject(azureBlazorWasmProjects);
                }
                else if (projectFile == null && functionProjectExists)
                {
                    projectFile = GetProject(azureFunctionsProjects);
                }
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
