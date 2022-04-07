// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector.DotNetCore
{
    internal class ProbeAndFindProjectFileProvider : IProjectFileProvider
    {
        private readonly ILogger<ProbeAndFindProjectFileProvider> logger;
        private readonly DetectorOptions options;

        private string projectFileRelativePath;

        public ProbeAndFindProjectFileProvider(
            ILogger<ProbeAndFindProjectFileProvider> logger,
            IOptions<DetectorOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;
        }

        public string GetRelativePathToProjectFile(DetectorContext context)
        {
            var sourceRepo = context.SourceRepo;
            string projectFile = null;

            // search for .csproj files
            var projectFiles = this.GetAllProjectFilesInRepo(
                    sourceRepo,
                    DotNetCoreConstants.CSharpProjectFileExtension);

            if (!projectFiles.Any())
            {
                this.logger.LogDebug(
                    "Could not find any files with extension " +
                    $"'{DotNetCoreConstants.CSharpProjectFileExtension}' in repo.");

                // search for .fsproj files
                projectFiles = this.GetAllProjectFilesInRepo(
                    sourceRepo,
                    DotNetCoreConstants.FSharpProjectFileExtension);

                if (!projectFiles.Any())
                {
                    this.logger.LogDebug(
                        "Could not find any files with extension " +
                        $"'{DotNetCoreConstants.FSharpProjectFileExtension}' in repo.");
                    return null;
                }
            }

            var webAppProjects = new List<string>();
            var azureFunctionsProjects = new List<string>();
            var blazorWasmProjects = new List<string>();
            var allProjects = new List<string>();

            foreach (var file in projectFiles)
            {
                allProjects.Add(file);
                if (ProjectFileHelpers.IsAzureBlazorWebAssemblyProject(sourceRepo, file))
                {
                    blazorWasmProjects.Add(file);
                }
                else if (ProjectFileHelpers.IsAzureFunctionsProject(sourceRepo, file))
                {
                    azureFunctionsProjects.Add(file);
                }
                else if (ProjectFileHelpers.IsAspNetCoreWebApplicationProject(sourceRepo, file))
                {
                    webAppProjects.Add(file);
                }
            }

            // Assumption: some build option "--apptype" will be passed to oryx
            // which will indicate if the app is an azure function type or static type app

            // If there are multiple projects, we will look for --apptype to detect corresponding project.
            // for example azurefunction and blazor both projects can reside
            // at the same repo, so more than 2 csprojs will be found. Now we will
            // look for --apptype value to determine which project needs to be built
            if (!string.IsNullOrEmpty(this.options.AppType))
            {
                this.logger.LogInformation($"{nameof(this.options.AppType)} is set to {this.options.AppType}");

                var appType = this.options.AppType.ToLower();
                if (appType.Contains(Constants.FunctionApplications))
                {
                    if (azureFunctionsProjects.Count == 0)
                    {
                        return null;
                    }

                    projectFile = GetProject(azureFunctionsProjects);
                }
                else if (appType.Contains(Constants.StaticSiteApplications))
                {
                    if (blazorWasmProjects.Count == 0)
                    {
                        return null;
                    }

                    projectFile = GetProject(blazorWasmProjects);
                }
                else if (appType.Contains(Constants.WebApplications))
                {
                    if (webAppProjects.Count == 0)
                    {
                        return null;
                    }

                    projectFile = GetProject(webAppProjects);
                }
                else
                {
                    this.logger.LogDebug($"Unrecognized app type {appType}'.");
                }
            }
            else
            {
                this.logger.LogInformation($"AppType is not provided. Selecting projects based on ");

                // If multiple project exists, and appType is not passed
                // we detect them in following order
                if (webAppProjects.Count > 0)
                {
                    projectFile = GetProject(webAppProjects);
                }
                else if (blazorWasmProjects.Count > 0)
                {
                    projectFile = GetProject(blazorWasmProjects);
                }
                else if (azureFunctionsProjects.Count > 0)
                {
                    projectFile = GetProject(azureFunctionsProjects);
                }
            }

            // After scanning all the project types we still didn't find any files (e.g. csproj
            if (projectFile == null)
            {
                this.logger.LogDebug("Could not find a .NET Core project file to build.");
                return null;
            }

            this.projectFileRelativePath = ProjectFileHelpers.GetRelativePathToRoot(projectFile, sourceRepo.RootPath);
            return this.projectFileRelativePath;
        }

        private static string GetProject(List<string> projects)
        {
            if (projects.Count > 1)
            {
                var projectList = string.Join(", ", projects);
                throw new InvalidProjectFileException(
                    $"Ambiguity in selecting a project to build. " +
                    $"Found multiple projects: {string.Join(", ", projectList)}.");
            }

            if (projects.Count == 1)
            {
                return projects[0];
            }

            return null;
        }

        private IEnumerable<string> GetAllProjectFilesInRepo(
            ISourceRepo sourceRepo,
            string projectFileExtension)
        {
            var searchSubDirectories = !this.options.DisableRecursiveLookUp;
            if (!searchSubDirectories)
            {
                this.logger.LogDebug("Skipping search for files in sub-directories as it has been disabled.");
            }

            return sourceRepo.EnumerateFiles($"*.{projectFileExtension}", searchSubDirectories);
        }
    }
}
