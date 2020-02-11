// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// .NET Core platform.
    /// </summary>
    [BuildProperty(
        DotNetCoreConstants.ProjectBuildPropertyKey,
        DotNetCoreConstants.ProjectBuildPropertyKeyDocumentation)]
    [BuildProperty(Constants.ZipAllOutputBuildPropertyKey, Constants.ZipAllOutputBuildPropertyKeyDocumentation)]
    internal class DotNetCorePlatform : IProgrammingPlatform
    {
        private readonly IDotNetCoreVersionProvider _versionProvider;
        private readonly DefaultProjectFileProvider _projectFileProvider;
        private readonly IEnvironmentSettingsProvider _environmentSettingsProvider;
        private readonly ILogger<DotNetCorePlatform> _logger;
        private readonly DotNetCoreLanguageDetector _detector;
        private readonly DotNetCoreScriptGeneratorOptions _dotNetCorePlatformOptions;
        private readonly BuildScriptGeneratorOptions _buildOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetCorePlatform"/> class.
        /// </summary>
        /// <param name="versionProvider">The .NET version provider.</param>
        /// <param name="projectFileProvider">The project file provider.</param>
        /// <param name="environmentSettingsProvider">The environment settings provider.</param>
        /// <param name="logger">The logger of .NET platform.</param>
        /// <param name="detector">The detector of .NET platform.</param>
        /// <param name="buildOptions">The build options for BuildScriptGenerator.</param>
        /// <param name="dotNetCorePlatformOptions">The options if .NET platform.</param>
        public DotNetCorePlatform(
            IDotNetCoreVersionProvider versionProvider,
            DefaultProjectFileProvider projectFileProvider,
            IEnvironmentSettingsProvider environmentSettingsProvider,
            ILogger<DotNetCorePlatform> logger,
            DotNetCoreLanguageDetector detector,
            IOptions<BuildScriptGeneratorOptions> buildOptions,
            IOptions<DotNetCoreScriptGeneratorOptions> dotNetCorePlatformOptions)
        {
            _versionProvider = versionProvider;
            _projectFileProvider = projectFileProvider;
            _environmentSettingsProvider = environmentSettingsProvider;
            _logger = logger;
            _detector = detector;
            _dotNetCorePlatformOptions = dotNetCorePlatformOptions.Value;
            _buildOptions = buildOptions.Value;
        }

        /// <summary>
        /// Gets the name of .NET platform which this generator will create builds for.
        /// </summary>
        public string Name => DotNetCoreConstants.LanguageName;

        /// <summary>
        /// Gets the list of versions that the script generator supports.
        /// </summary>
        public IEnumerable<string> SupportedVersions => _versionProvider.SupportedDotNetCoreVersions;

        /// <summary>
        /// Detects the programming platform name and version required by the application in source directory.
        /// </summary>
        /// <param name="context">The repository context.</param>
        /// <returns>The results of language detector operations.</returns>
        public LanguageDetectorResult Detect(RepositoryContext context)
        {
            return _detector.Detect(context);
        }

        /// <summary>
        /// Generates a build Bash script based on the application in source directory.
        /// </summary>
        /// <param name="context">The context for Build Script Generator.</param>
        /// <returns>The build script snippet.</returns>
        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext context)
        {
            var buildProperties = new Dictionary<string, string>();

            // Write the version to the manifest file
            var key = $"{DotNetCoreConstants.LanguageName}_version";
            buildProperties[key] = context.DotNetCoreVersion;

            buildProperties[ManifestFilePropertyKeys.OperationId] = context.OperationId;

            var projectFile = _projectFileProvider.GetRelativePathToProjectFile(context);
            if (string.IsNullOrEmpty(projectFile))
            {
                return null;
            }

            _environmentSettingsProvider.TryGetAndLoadSettings(out var environmentSettings);

            (var preBuildCommand, var postBuildCommand) = PreAndPostBuildCommandHelper.GetPreAndPostBuildCommands(
                context.SourceRepo,
                environmentSettings);

            var sourceDir = _buildOptions.SourceDir;
            var temporaryDestinationDir = "/tmp/puboutput";
            var destinationDir = _buildOptions.DestinationDir;
            var intermediateDir = _buildOptions.IntermediateDir;
            var hasUserSuppliedDestinationDir = !string.IsNullOrEmpty(_buildOptions.DestinationDir);
            var zipAllOutput = ShouldZipAllOutput(context);
            var buildConfiguration = GetBuildConfiguration();

            var scriptBuilder = new StringBuilder();
            scriptBuilder
                .AppendLine("#!/bin/bash")
                .AppendLine("set -e")
                .AppendLine()
                .AddScriptToCopyToIntermediateDirectory(
                    sourceDir: ref sourceDir,
                    intermediateDir: intermediateDir,
                    GetDirectoriesToExcludeFromCopyToIntermediateDir(context))
                .AppendFormatWithLine("cd \"{0}\"", sourceDir)
                .AppendLine();

            scriptBuilder
                .AddScriptToSetupSourceAndDestinationDirectories(
                    sourceDir: sourceDir,
                    temporaryDestinationDir: temporaryDestinationDir,
                    destinationDir: destinationDir,
                    hasUserSuppliedDestinationDir: hasUserSuppliedDestinationDir,
                    zipAllOutput: zipAllOutput)
                .AppendBenvCommand($"dotnet={context.DotNetCoreVersion}")
                .AddScriptToRunPreBuildCommand(sourceDir: sourceDir, preBuildCommand: preBuildCommand)
                .AppendLine("echo")
                .AppendLine("dotnetCoreVersion=$(dotnet --version)")
                .AppendLine("echo \"Using .NET Core SDK Version: $dotnetCoreVersion\"")
                .AppendLine()
                .AddScriptToRestorePackages(projectFile);

            if (hasUserSuppliedDestinationDir)
            {
                if (zipAllOutput)
                {
                    scriptBuilder.AddScriptToZipAllOutput(
                        projectFile: projectFile,
                        buildConfiguration: buildConfiguration,
                        sourceDir: sourceDir,
                        temporaryDestinationDir: temporaryDestinationDir,
                        finalDestinationDir: destinationDir,
                        postBuildCommand: postBuildCommand,
                        buildProperties);
                }
                else
                {
                    scriptBuilder
                        .AddScriptToPublishOutput(
                            projectFile: projectFile,
                            buildConfiguration: buildConfiguration,
                            finalDestinationDir: destinationDir)
                        .AddScriptToRunPostBuildCommand(
                            sourceDir: sourceDir,
                            postBuildCommand: postBuildCommand);
                }
            }
            else
            {
                scriptBuilder
                    .AddScriptToBuildProject(projectFile)
                    .AddScriptToRunPostBuildCommand(
                        sourceDir: sourceDir,
                        postBuildCommand: postBuildCommand);
            }

            SetStartupFileNameInfoInManifestFile(context, projectFile, buildProperties);

            scriptBuilder
                .AddScriptToCreateManifestFile(
                    buildProperties,
                    manifestDir: _buildOptions.ManifestDir,
                    finalDestinationDir: destinationDir)
                .AppendLine("echo Done.");

            return new BuildScriptSnippet
            {
                BashBuildScriptSnippet = scriptBuilder.ToString(),
                IsFullScript = true,
            };
        }

        /// <summary>
        /// Checks if the source repository seems to have artifacts from a previous build.
        /// </summary>
        /// <param name="repo">A source code repository.</param>
        /// <returns>True if the source repository have artifacts already, False otherwise.</returns>
        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        /// <summary>
        /// Generates a bash script that can install the required runtime bits for the application's platforms.
        /// </summary>
        /// <param name="options">The options for runtime installation script generator.</param>
        /// <exception cref="NotImplementedException">Thrown when it's not implemented.</exception>
        /// <returns>Message from exception.</returns>
        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the programming platform should be included in a build script.
        /// </summary>
        /// <param name="ctx">The repository context.</param>
        /// <returns>True if the programming platform should be included in a build script, False otherwise.</returns>
        public bool IsEnabled(RepositoryContext ctx)
        {
            return ctx.EnableDotNetCore;
        }

        /// <summary>
        /// Checks if the programming platform wants to participate in a multi-platform build.
        /// </summary>
        /// <param name="ctx">The repository context.</param>
        /// <returns>True if the programming platform is enabled for multi-platform build, False otherwise.</returns>
        public bool IsEnabledForMultiPlatformBuild(RepositoryContext ctx)
        {
            // A user has the power to either enable or disable multi-platform builds entirely.
            // However if user enables it, ASP.NET Core platform still explicitly opts out of it.
            return false;
        }

        /// <summary>
        /// Adds the required tools and their versions to a map.
        /// </summary>
        /// <param name="sourceRepo">The source repository.</param>
        /// <param name="targetPlatformVersion">The version of .NET platform.</param>
        /// <param name="toolsToVersion">A dictionary with tools as keys and versions as values.</param>
        public void SetRequiredTools(
            ISourceRepo sourceRepo,
            string targetPlatformVersion,
            IDictionary<string, string> toolsToVersion)
        {
            Debug.Assert(toolsToVersion != null, $"{nameof(toolsToVersion)} must not be null.");
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion[DotNetCoreConstants.LanguageName] = targetPlatformVersion;
            }
        }

        /// <summary>
        /// Sets the version of the .NET platform in BuildScriptGeneratorContext.
        /// </summary>
        /// <param name="context">The context of BuildScriptGenerator.</param>
        /// <param name="version">The version of the .NET platform.</param>
        public void SetVersion(BuildScriptGeneratorContext context, string version)
        {
            context.DotNetCoreVersion = version;
        }

        /// <summary>
        /// Gets list of directories which need to be excluded from being copied to the output directory.
        /// </summary>
        /// <param name="scriptGeneratorContext">The context of BuildScriptGenerator.</param>
        /// <returns>A list of directories.</returns>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            var dirs = new List<string>();
            dirs.Add("obj");
            dirs.Add("bin");
            return dirs;
        }

        /// <summary>
        /// Gets list of directories which need to be excluded from being copied to the intermediate directory, if used.
        /// </summary>
        /// <param name="scriptGeneratorContext">The context of BuildScriptGenerator.</param>
        /// <returns>A list of directories.</returns>
        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            var dirs = new List<string>();
            dirs.Add(".git");
            dirs.Add("obj");
            dirs.Add("bin");
            return dirs;
        }

        private string GetBuildConfiguration()
        {
            var configuration = _dotNetCorePlatformOptions.MSBuildConfiguration;
            if (string.IsNullOrEmpty(configuration))
            {
                configuration = DotNetCoreConstants.DefaultMSBuildConfiguration;
            }

            return configuration;
        }

        private bool ShouldZipAllOutput(BuildScriptGeneratorContext context)
        {
            return BuildPropertiesHelper.IsTrue(
                Constants.ZipAllOutputBuildPropertyKey,
                context,
                valueIsRequired: false);
        }

        /// <summary>
        /// Even though the runtime container has the logic of finding out the startup file based on
        /// 'runtimeconfig.json' prefix, we still set the name in the manifest file because of the following
        /// scenario: let's say output directory currently has 'foo.dll' and user made a change to the project
        /// name or assembly name property to 'bar' which causes 'bar.dll' to be published. If the output
        /// directory was NOT cleaned, then we would now be having both 'foo.runtimeconfig.json' and
        /// 'bar.runtimeconfig.json' which causes a problem for runtime container as it cannot figure out the
        /// right startup dll. So, to help that scenario we always set the start-up file name in manifest file.
        /// The runtime container will first look into manifest file to find the startup filename, if the
        /// file name is not present or if a manifest file is not present at all(ex: in case of VS Publish where
        /// the build does not happen with Oryx), then the runtime container's logic will fallback to looking at
        /// runtimeconfig.json prefixes.
        /// </summary>
        private void SetStartupFileNameInfoInManifestFile(
            BuildScriptGeneratorContext context,
            string projectFile,
            IDictionary<string, string> buildProperties)
        {
            string startupDllFileName;
            var projectFileContent = context.SourceRepo.ReadFile(projectFile);
            var projFileDoc = XDocument.Load(new StringReader(projectFileContent));
            var assemblyNameElement = projFileDoc.XPathSelectElement(DotNetCoreConstants.AssemblyNameXPathExpression);
            if (assemblyNameElement == null)
            {
                var name = Path.GetFileNameWithoutExtension(projectFile);
                startupDllFileName = $"{name}.dll";
            }
            else
            {
                startupDllFileName = $"{assemblyNameElement.Value}.dll";
            }

            buildProperties[DotNetCoreManifestFilePropertyKeys.StartupDllFileName] = startupDllFileName;
        }
    }
}