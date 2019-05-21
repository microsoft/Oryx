// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private string _projectFileRelativePath;

        public DefaultAspNetCoreWebAppProjectFileProvider(
            IOptions<DotnetCoreScriptGeneratorOptions> options,
            ILogger<DefaultAspNetCoreWebAppProjectFileProvider> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public string GetRelativePathToProjectFile(ISourceRepo sourceRepo)
        {
            if (_probedForProjectFile)
            {
                return _projectFileRelativePath;
            }

            var projectEnvVariablePath = _options.Project;

            string projectFile = null;
            if (!string.IsNullOrEmpty(projectEnvVariablePath))
            {
                var projectFileWithRelativePath = projectEnvVariablePath.Trim();
                projectFile = Path.Combine(sourceRepo.RootPath, projectFileWithRelativePath);
                if (!sourceRepo.FileExists(projectFile))
                {
                    _logger.LogWarning($"Could not find the project file '{projectFile}'.");
                    throw new InvalidUsageException(
                        $"Could not find the project file '{projectFile}' specified by the environment variable" +
                        $" '{EnvironmentSettingsKeys.Project}' with value '{projectFileWithRelativePath}'. " +
                        "Make sure the path to the project file is relative to the root of the repo. " +
                        "For example: PROJECT=src/Dashboard/Dashboard.csproj");
                }

                // NOTE: Do not check if the project file specified by the end user is a web application since this
                // can be a escape hatch for end users if our logic to determine a web app is incorrect.
                return projectFileWithRelativePath;
            }

            // Check if root of the repo has a .csproj or a .fsproj file
            projectFile = GetProjectFileAtRoot(sourceRepo, DotnetCoreConstants.CSharpProjectFileExtension) ??
                GetProjectFileAtRoot(sourceRepo, DotnetCoreConstants.FSharpProjectFileExtension);

            if (projectFile != null)
            {
                if (!IsAspNetCoreWebApplicationProject(sourceRepo, projectFile))
                {
                    return null;
                }

                return new FileInfo(projectFile).Name;
            }

            // Check if any of the sub-directories has a .csproj file and if that .csproj file has references
            // to web sdk.
            if (projectFile == null)
            {
                // search for .csproj files
                var projectFiles = GetAllProjectFilesInRepo(
                        sourceRepo,
                        DotnetCoreConstants.CSharpProjectFileExtension);

                if (!projectFiles.Any())
                {
                    _logger.LogDebug(
                        "Could not find any files with extension " +
                        $"'{DotnetCoreConstants.CSharpProjectFileExtension}' in repo.");

                    // search for .fsproj files
                    projectFiles = GetAllProjectFilesInRepo(
                        sourceRepo,
                        DotnetCoreConstants.FSharpProjectFileExtension);

                    if (!projectFiles.Any())
                    {
                        _logger.LogDebug(
                            "Could not find any files with extension " +
                            $"'{DotnetCoreConstants.FSharpProjectFileExtension}' in repo.");
                        return null;
                    }
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
            _projectFileRelativePath = GetRelativePathToRoot(projectFile, sourceRepo.RootPath);
            return _projectFileRelativePath;
        }

        // To enable unit testing
        internal static bool IsAspNetCoreWebApplicationProject(XDocument projectFileDoc)
        {
            // For reference
            // https://docs.microsoft.com/en-us/visualstudio/msbuild/project-element-msbuild?view=vs-2019

            // Look for the attribute value on Project element first as that is more common
            // Example: <Project Sdk="Microsoft.NET.Sdk.Web/1.0.0">
            var expectedWebSdkName = DotnetCoreConstants.WebSdkName.ToLowerInvariant();
            var sdkAttributeValue = projectFileDoc.XPathEvaluate(
                DotnetCoreConstants.ProjectSdkAttributeValueXPathExpression);
            var sdkName = sdkAttributeValue as string;
            if (!string.IsNullOrEmpty(sdkName) &&
                sdkName.StartsWith(expectedWebSdkName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Example:
            // <Project>
            //    <Sdk Name="Microsoft.NET.Sdk.Web" Version="1.0.0" />
            var sdkNameAttributeValue = projectFileDoc.XPathEvaluate(
                DotnetCoreConstants.ProjectSdkElementNameAttributeValueXPathExpression);
            sdkName = sdkNameAttributeValue as string;

            return string.Equals(sdkName, expectedWebSdkName, StringComparison.OrdinalIgnoreCase);
        }

        // To enable unit testing
        internal static string GetRelativePathToRoot(string projectFilePath, string repoRoot)
        {
            var repoRootDir = new DirectoryInfo(repoRoot);
            var projectFileInfo = new FileInfo(projectFilePath);
            var currDir = projectFileInfo.Directory;
            var parts = new List<string>();
            parts.Add(projectFileInfo.Name);

            // Since directory names are case sensitive on non-Windows OSes, try not to use ignore case
            while (!string.Equals(currDir.FullName, repoRootDir.FullName, StringComparison.Ordinal))
            {
                parts.Insert(0, currDir.Name);
                currDir = currDir.Parent;
            }

            return Path.Combine(parts.ToArray());
        }

        private static IEnumerable<string> GetAllProjectFilesInRepo(
            ISourceRepo sourceRepo,
            string projectFileExtension)
        {
            return sourceRepo.EnumerateFiles($"*.{projectFileExtension}", searchSubDirectories: true);
        }

        private static string GetProjectFileAtRoot(ISourceRepo sourceRepo, string projectFileExtension)
        {
            return sourceRepo
                .EnumerateFiles($"*.{projectFileExtension}", searchSubDirectories: false)
                .FirstOrDefault();
        }

        private static bool IsAspNetCore30App(XDocument projectFileDoc)
        {
            var targetFrameworkElement = projectFileDoc.XPathSelectElement(
                DotnetCoreConstants.TargetFrameworkElementXPathExpression);
            if (string.Equals(targetFrameworkElement.Value, DotnetCoreConstants.NetCoreApp30))
            {
                var projectElement = projectFileDoc.XPathSelectElement(
                    DotnetCoreConstants.ProjectSdkAttributeValueXPathExpression);
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
