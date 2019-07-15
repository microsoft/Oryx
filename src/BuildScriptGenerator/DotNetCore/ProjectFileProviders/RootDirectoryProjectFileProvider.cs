// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// Gets the relative path to a project file, if it is present at the root of the given source directory.
    /// </summary>
    internal class RootDirectoryProjectFileProvider : IProjectFileProvider
    {
        private readonly ILogger<RootDirectoryProjectFileProvider> _logger;

        public RootDirectoryProjectFileProvider(ILogger<RootDirectoryProjectFileProvider> logger)
        {
            _logger = logger;
        }

        public string GetRelativePathToProjectFile(BuildScriptGeneratorContext context)
        {
            var sourceRepo = context.SourceRepo;

            // Check if root of the repo has a .csproj or a .fsproj file
            var projectFile = GetProjectFileAtRoot(sourceRepo, DotNetCoreConstants.CSharpProjectFileExtension) ??
                GetProjectFileAtRoot(sourceRepo, DotNetCoreConstants.FSharpProjectFileExtension);

            if (projectFile != null)
            {
                var isCompatibleProject = false;
                if (ProjectFileHelpers.IsAspNetCoreWebApplicationProject(context.SourceRepo, projectFile))
                {
                    _logger.LogDebug($"Project '{projectFile}' is an ASP.NET Core Web Application project.");
                    isCompatibleProject = true;
                }
                else
                {
                    _logger.LogDebug($"Project '{projectFile}' is not an ASP.NET Core Web Application project.");
                }

                if (!isCompatibleProject &&
                    ProjectFileHelpers.IsAzureFunctionsProject(context.SourceRepo, projectFile))
                {
                    _logger.LogDebug($"Project '{projectFile}' is an Azure Functions project.");
                    isCompatibleProject = true;
                }
                else
                {
                    _logger.LogDebug($"Project '{projectFile}' is not an Azure Functions project.");
                }

                if (isCompatibleProject)
                {
                    return new FileInfo(projectFile).Name;
                }
            }

            return null;
        }

        private static string GetProjectFileAtRoot(ISourceRepo sourceRepo, string projectFileExtension)
        {
            return sourceRepo
                .EnumerateFiles($"*.{projectFileExtension}", searchSubDirectories: false)
                .FirstOrDefault();
        }
    }
}
