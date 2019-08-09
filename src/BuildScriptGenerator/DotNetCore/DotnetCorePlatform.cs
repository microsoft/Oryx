// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common;

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

        public LanguageDetectorResult Detect(BuildScriptGeneratorContext context)
        {
            return _detector.Detect(context);
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext context)
        {
            var buildProperties = new Dictionary<string, string>();
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

            var scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine("#!/bin/bash");
            scriptBuilder.AppendLine("set -e");
            scriptBuilder.AppendLine();

            var sourceDir = _buildOptions.SourceDir;
            var destinationDir = _buildOptions.DestinationDir;
            var userSuppliedDestinationDir = !string.IsNullOrEmpty(_buildOptions.DestinationDir);
            var zipAllOutput = ShouldZipAllOutput();

            AddScriptToCopyToIntermediateDirectory();

            AddScriptToSetupSourceAndDestinationDirectories();

            scriptBuilder.AppendBenvCommand($"dotnet={context.DotNetCoreVersion}");

            AddScriptToRunPreBuildCommand();

            scriptBuilder.AppendLine("echo");
            scriptBuilder.AppendLine("dotnetCoreVersion=$(dotnet --version)");
            scriptBuilder.AppendLine("echo \"Using .NET Core SDK Version: $dotnetCoreVersion\"");
            scriptBuilder.AppendLine();

            AddScriptToRestorePackages();

            if (userSuppliedDestinationDir)
            {
                if (zipAllOutput)
                {
                    AddScriptToZipAllOutput();
                }
                else
                {
                    AddScriptToPublishOutput();

                    AddScriptToRunPostBuildCommand();
                }
            }
            else
            {
                AddScriptToBuildProject();

                AddScriptToRunPostBuildCommand();
            }

            SetStartupFileNameInfoInManifestFile(context, projectFile, buildProperties);

            AddScriptToCreateManifestFile();

            scriptBuilder.AppendLine("echo Done.");

            return new BuildScriptSnippet
            {
                BashBuildScriptSnippet = scriptBuilder.ToString(),
                IsFullScript = true,
            };

            void AddScriptToCopyToIntermediateDirectory()
            {
                if (!string.IsNullOrEmpty(_buildOptions.IntermediateDir))
                {
                    var intermediateDir = _buildOptions.IntermediateDir;
                    if (!Directory.Exists(intermediateDir))
                    {
                        scriptBuilder.AppendLine();
                        scriptBuilder.AppendLine("echo Intermediate directory does not exist, creating it...");
                        scriptBuilder.AppendFormatWithLine("mkdir -p \"{0}\"", intermediateDir);
                    }

                    scriptBuilder.AppendLine();
                    scriptBuilder.AppendFormatWithLine("cd \"{0}\"", _buildOptions.SourceDir);
                    scriptBuilder.AppendLine("echo");
                    var excludeDirs = GetDirectoriesToExcludeFromCopyToIntermediateDir(context);
                    var excludeDirsSwitch = string.Join(" ", excludeDirs.Select(dir => $"--exclude \"{dir}\""));
                    scriptBuilder.AppendFormatWithLine(
                        "rsync --delete -rt {0} . \"{1}\"",
                        excludeDirsSwitch,
                        intermediateDir);
                    sourceDir = intermediateDir;
                    scriptBuilder.AppendFormatWithLine("cd \"{0}\"", sourceDir);
                }
            }

            void AddScriptToRunPreBuildCommand()
            {
                if (!string.IsNullOrEmpty(preBuildCommand))
                {
                    scriptBuilder.AppendLine();
                    scriptBuilder.AppendFormatWithLine("cd \"{0}\"", sourceDir);
                    scriptBuilder.AppendLine(preBuildCommand);
                    scriptBuilder.AppendLine();
                }
            }

            void AddScriptToRunPostBuildCommand()
            {
                if (!string.IsNullOrEmpty(postBuildCommand))
                {
                    scriptBuilder.AppendLine();
                    scriptBuilder.AppendFormatWithLine("cd \"{0}\"", sourceDir);
                    scriptBuilder.AppendLine(postBuildCommand);
                    scriptBuilder.AppendLine();
                }
            }

            void AddScriptToSetupSourceAndDestinationDirectories()
            {
                scriptBuilder.AppendFormatWithLine("SOURCE_DIR=\"{0}\"", sourceDir);
                scriptBuilder.AppendLine("export SOURCE_DIR");

                if (userSuppliedDestinationDir)
                {
                    scriptBuilder.AppendLine("echo");
                    scriptBuilder
                        .AppendSourceDirectoryInfo(sourceDir)
                        .AppendDestinationDirectoryInfo(destinationDir);
                    scriptBuilder.AppendLine("echo");
                    scriptBuilder.AppendFormatWithLine("mkdir -p \"{0}\"", destinationDir);

                    if (zipAllOutput)
                    {
                        var tempOutputDir = "/tmp/puboutput";
                        var tempPublishDir = $"{tempOutputDir}/publish";
                        destinationDir = tempOutputDir;
                    }

                    scriptBuilder.AppendFormatWithLine("DESTINATION_DIR=\"{0}\"", _buildOptions.DestinationDir);
                    scriptBuilder.AppendLine("export DESTINATION_DIR");
                }
                else
                {
                    scriptBuilder.AppendLine("echo");
                    scriptBuilder.AppendSourceDirectoryInfo(sourceDir);
                    scriptBuilder.AppendLine("echo");
                }
            }

            void AddScriptToZipAllOutput()
            {
                var zipFileName = FilePaths.CompressedOutputFileName;

                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine("echo");
                scriptBuilder.AppendFormatWithLine("echo \"Publishing output to '{0}'\"", destinationDir);
                scriptBuilder.AppendFormatWithLine(
                    "dotnet publish \"{0}\" -c {1} -o \"{2}\"",
                    projectFile,
                    GetBuildConfiguration(),
                    destinationDir);

                AddScriptToRunPostBuildCommand();

                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine("echo Compressing the contents of the output directory...");
                scriptBuilder.AppendFormatWithLine("cd \"{0}\"", destinationDir);
                scriptBuilder.AppendFormatWithLine("tar -zcf ../{0} .", zipFileName);
                scriptBuilder.AppendLine("cd ..");
                scriptBuilder.AppendFormatWithLine(
                    "cp -f \"{0}\" \"{1}/{2}\"",
                    zipFileName,
                    _buildOptions.DestinationDir,
                    zipFileName);

                buildProperties[ManifestFilePropertyKeys.ZipAllOutput] = "true";
            }

            void AddScriptToCreateManifestFile()
            {
                if (buildProperties.Any())
                {
                    var manifestFileDir = context.ManifestDir;
                    if (string.IsNullOrEmpty(manifestFileDir))
                    {
                        manifestFileDir = _buildOptions.DestinationDir;
                    }

                    if (!string.IsNullOrEmpty(manifestFileDir))
                    {
                        scriptBuilder.AppendLine();
                        scriptBuilder.AppendFormatWithLine("mkdir -p \"{0}\"", manifestFileDir);
                        scriptBuilder.AppendLine("echo");
                        scriptBuilder.AppendLine("echo Removing any existing manifest file...");
                        scriptBuilder.AppendFormatWithLine(
                            "rm -f \"{0}/{1}\"",
                            manifestFileDir,
                            FilePaths.BuildManifestFileName);
                        scriptBuilder.AppendLine("echo Creating a manifest file...");

                        foreach (var property in buildProperties)
                        {
                            scriptBuilder.AppendFormatWithLine(
                                "echo '{0}=\"{1}\"' >> \"{2}/{3}\"",
                                property.Key,
                                property.Value,
                                manifestFileDir,
                                FilePaths.BuildManifestFileName);
                        }

                        scriptBuilder.AppendLine("echo Manifest file created.");
                    }
                }
            }

            void AddScriptToRestorePackages()
            {
                scriptBuilder.AppendLine("echo");
                scriptBuilder.AppendLine("echo Restoring packages...");
                scriptBuilder.AppendFormatWithLine("dotnet restore \"{0}\"", projectFile);
            }

            void AddScriptToBuildProject()
            {
                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine("echo");
                scriptBuilder.AppendFormatWithLine("echo \"Building project '{0}'\"", projectFile);

                // Use the default build configuration 'Debug' here.
                scriptBuilder.AppendFormatWithLine("dotnet build \"{0}\"", projectFile);
            }

            void AddScriptToPublishOutput()
            {
                scriptBuilder.AppendLine();
                scriptBuilder.AppendFormatWithLine(
                    "echo \"Publishing output to '{0}'\"",
                    _buildOptions.DestinationDir);
                scriptBuilder.AppendFormatWithLine(
                    "dotnet publish \"{0}\" -c {1} -o \"{2}\"",
                    projectFile,
                    GetBuildConfiguration(),
                    _buildOptions.DestinationDir);
            }

            bool ShouldZipAllOutput()
            {
                return BuildPropertiesHelper.IsTrue(
                    Constants.ZipAllOutputBuildPropertyKey,
                    context,
                    valueIsRequired: false);
            }
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

        public bool IsEnabled(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return scriptGeneratorContext.EnableDotNetCore;
        }

        public bool IsEnabledForMultiPlatformBuild(BuildScriptGeneratorContext scriptGeneratorContext)
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
    }
}