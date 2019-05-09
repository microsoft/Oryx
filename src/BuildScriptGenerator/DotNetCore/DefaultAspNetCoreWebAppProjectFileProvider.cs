// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DefaultAspNetCoreWebAppProjectFileProvider : IAspNetCoreWebAppProjectFileProvider
    {
        private readonly DotnetCoreScriptGeneratorOptions _options;
        private readonly ILogger<DefaultAspNetCoreWebAppProjectFileProvider> _logger;

        // Since this service is registered as a singleton, we can cache the lookup of project file.
        private bool _probedForProjectFile;
        private string _projectFile;

        public DefaultAspNetCoreWebAppProjectFileProvider(
            IOptions<DotnetCoreScriptGeneratorOptions> options,
            ILogger<DefaultAspNetCoreWebAppProjectFileProvider> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public string GetProjectFile(ISourceRepo sourceRepo)
        {
            if (_probedForProjectFile)
            {
                return _projectFile;
            }

            var projectEnvVariablePath = _options.Project;

            string projectFile = null;
            if (string.IsNullOrEmpty(projectEnvVariablePath))
            {
                // Check if root of the repo has a .csproj file
                projectFile = sourceRepo
                    .EnumerateFiles($"*.{DotnetCoreConstants.ProjectFileExtensionName}", searchSubDirectories: false)
                    .FirstOrDefault();
            }
            else
            {
                projectEnvVariablePath = projectEnvVariablePath.Trim();
                projectFile = Path.Combine(sourceRepo.RootPath, projectEnvVariablePath);
                if (!sourceRepo.FileExists(projectFile))
                {
                    _logger.LogWarning($"Could not find the project file '{projectFile}'.");
                    throw new InvalidUsageException(
                        $"Could not find the project file '{projectFile}' specified by the environment variable" +
                        $" '{EnvironmentSettingsKeys.Project}' with value '{projectEnvVariablePath}'. " +
                        "Make sure the path to the project file is relative to the root of the repo. " +
                        "For example: PROJECT=src/Dashboard/Dashboard.csproj");
                }

                // NOTE: Do not check if the project file specified by the end user is a web application since this
                // can be a escape hatch for end users if our logic to determine a web app is incorrect.
                return projectFile;
            }

            if (projectFile != null)
            {
                if (!IsAspNetCoreWebApplicationProject(sourceRepo, projectFile))
                {
                    return null;
                }

                return projectFile;
            }

            // Check if any of the sub-directories has a .csproj file and if that .csproj file has references
            // to web sdk.
            if (projectFile == null)
            {
                var projectFiles = sourceRepo
                    .EnumerateFiles($"*.{DotnetCoreConstants.ProjectFileExtensionName}", searchSubDirectories: true);

                if (!projectFiles.Any())
                {
                    _logger.LogDebug(
                        "Could not find any files with extension " +
                        $"'{DotnetCoreConstants.ProjectFileExtensionName}' in repo.");
                    return null;
                }

                var webAppProjects = new List<string>();
                foreach (var file in projectFiles)
                {
                    if (IsAspNetCoreWebApplicationProject(sourceRepo, file))
                    {
                        webAppProjects.Add(file);
                    }
                }

                if (webAppProjects.Count == 0)
                {
                    _logger.LogDebug(
                        "Could not find any ASP.NET Core web application projects. " +
                        $"Found the following project files: '{string.Join(" ", projectFiles)}'");
                    return null;
                }

                if (webAppProjects.Count > 1)
                {
                    var projects = string.Join(", ", webAppProjects);
                    throw new InvalidUsageException(
                        "Ambiguity in selecting an ASP.NET Core web application to build. " +
                        $"Found multiple applications: '{projects}'. Use the environment variable " +
                        $"'{EnvironmentSettingsKeys.Project}' to specify the relative path to the project " +
                        "to be deployed.");
                }

                projectFile = webAppProjects[0];
            }

            // Cache the results
            _probedForProjectFile = true;
            _projectFile = projectFile;

            return _projectFile;
        }

        // To enable unit testing
        internal static bool IsAspNetCoreWebApplicationProject(XDocument projectFileDoc)
        {
            var webSdkProjectElement = projectFileDoc.XPathSelectElement(
                DotnetCoreConstants.WebSdkProjectXPathExpression);
            return webSdkProjectElement != null;
        }

        private static bool IsAspNetCore30App(XDocument projectFileDoc)
        {
            var targetFrameworkElement = projectFileDoc.XPathSelectElement(
                DotnetCoreConstants.TargetFrameworkXPathExpression);
            if (string.Equals(targetFrameworkElement.Value, DotnetCoreConstants.NetCoreApp30))
            {
                var projectElement = projectFileDoc.XPathSelectElement(
                    DotnetCoreConstants.WebSdkProjectXPathExpression);
                return projectElement != null;
            }

            return false;
        }

        private bool IsAspNetCoreWebApplicationProject(ISourceRepo sourceRepo, string projectFile)
        {
            var projFileDoc = XDocument.Load(new StringReader(sourceRepo.ReadFile(projectFile)));
            return IsAspNetCoreWebApplicationProject(projFileDoc);
        }
    }
}
