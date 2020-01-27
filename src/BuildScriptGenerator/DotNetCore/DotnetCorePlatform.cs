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

        public string Name => DotNetCoreConstants.LanguageName;

        public IEnumerable<string> SupportedVersions => _versionProvider.SupportedDotNetCoreVersions;

        public LanguageDetectorResult Detect(RepositoryContext context)
        {
            return _detector.Detect(context);
        }

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

        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(RepositoryContext ctx)
        {
            return ctx.EnableDotNetCore;
        }

        public bool IsEnabledForMultiPlatformBuild(RepositoryContext ctx)
        {
            // A user has the power to either enable or disable multi-platform builds entirely.
            // However if user enables it, ASP.NET Core platform still explicitly opts out of it.
            return false;
        }

        public void SetRequiredTools(
            ISourceRepo sourceRepo,
            string targetPlatformVersion,
            IDictionary<string, string> toolsToVersion)
        {
            Debug.Assert(toolsToVersion != null, $"{nameof(toolsToVersion)} must not be null.");
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion["dotnet"] = targetPlatformVersion;
            }
        }

        public void SetVersion(BuildScriptGeneratorContext context, string version)
        {
            context.DotNetCoreVersion = version;
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            var dirs = new List<string>();
            dirs.Add("obj");
            dirs.Add("bin");
            return dirs;
        }

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

        bool ShouldZipAllOutput(BuildScriptGeneratorContext context)
        {
            return BuildPropertiesHelper.IsTrue(
                Constants.ZipAllOutputBuildPropertyKey,
                context,
                valueIsRequired: false);
        }
    }
}