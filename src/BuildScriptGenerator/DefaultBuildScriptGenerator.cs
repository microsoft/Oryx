// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Common.Extensions;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Extensibility;
using Microsoft.Oryx.BuildScriptGenerator.Resources;
using Microsoft.Oryx.Common.Extensions;
using Microsoft.Oryx.Detector;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Finds and resolves scripts generators based on user input and invokes one of them to generate a script.
    /// </summary>
    internal class DefaultBuildScriptGenerator : IBuildScriptGenerator
    {
        private readonly BuildScriptGeneratorOptions cliOptions;
        private readonly ICompatiblePlatformDetector compatiblePlatformDetector;
        private readonly DefaultPlatformsInformationProvider platformsInformationProvider;
        private readonly PlatformsInstallationScriptProvider environmentSetupScriptProvider;
        private readonly IEnumerable<IChecker> checkers;
        private readonly ILogger<DefaultBuildScriptGenerator> logger;
        private readonly IStandardOutputWriter writer;
        private readonly TelemetryClient telemetryClient;

        public DefaultBuildScriptGenerator(
            DefaultPlatformsInformationProvider platformsInformationProvider,
            PlatformsInstallationScriptProvider environmentSetupScriptProvider,
            IOptions<BuildScriptGeneratorOptions> cliOptions,
            ICompatiblePlatformDetector compatiblePlatformDetector,
            IEnumerable<IChecker> checkers,
            ILogger<DefaultBuildScriptGenerator> logger,
            IStandardOutputWriter writer,
            TelemetryClient telemetryClient)
        {
            this.platformsInformationProvider = platformsInformationProvider;
            this.environmentSetupScriptProvider = environmentSetupScriptProvider;
            this.cliOptions = cliOptions.Value;
            this.compatiblePlatformDetector = compatiblePlatformDetector;
            this.logger = logger;
            this.checkers = checkers;
            this.writer = writer;
            this.logger.LogDebug("Available checkers: {checkerCount}", this.checkers?.Count() ?? 0);
            this.telemetryClient = telemetryClient;
        }

        public void GenerateBashScript(
            BuildScriptGeneratorContext context,
            out string script,
            List<ICheckerMessage> checkerMessageSink = null)
        {
            script = null;

            IList<BuildScriptSnippet> buildScriptSnippets;
            var directoriesToExcludeFromCopyToIntermediateDir = new List<string>();
            var directoriesToExcludeFromCopyToBuildOutputDir = new List<string>();

            // Try detecting ALL platforms since in some scenarios this is required.
            // For example, in case of a multi-platform app like ASP.NET Core + NodeJs, we might need to dynamically
            // install both these platforms' sdks before actually using any of their commands. So even though a user
            // of Oryx might explicitly supply the platform of the app as .NET Core, we still need to make sure the
            // build environment is setup with detected platforms' sdks.
            var platformInfos = this.platformsInformationProvider.GetPlatformsInfo(context);
            var detectionResults = platformInfos.Select(pi => pi.DetectorResult);
            var installationScript = this.environmentSetupScriptProvider.GetBashScriptSnippet(
                context,
                detectionResults);

            // Get list of tools to be set on benv
            var toolsToVersion = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var toolsToBeSetInPath in platformInfos
                .Where(pi => pi.RequiredToolsInPath != null)
                .Select(pi => pi.RequiredToolsInPath))
            {
                foreach (var toolNameAndVersion in toolsToBeSetInPath)
                {
                    if (!string.IsNullOrEmpty(
                        Environment.GetEnvironmentVariable(toolNameAndVersion.Key)))
                    {
                        this.logger.LogInformation($"If {toolNameAndVersion.Key} is set as environment, it'll be not be set via benv");
                    }
                    else
                    {
                        this.logger.LogInformation($"If {toolNameAndVersion.Key} is not set as environment, it'll be set to {toolNameAndVersion.Value} via benv");
                        toolsToVersion[toolNameAndVersion.Key] = toolNameAndVersion.Value;
                    }
                }
            }

            using (var timedEvent = this.telemetryClient.LogTimedEvent("GetBuildSnippets"))
            {
                buildScriptSnippets = this.GetBuildSnippets(
                    context,
                    detectionResults,
                    runDetection: false,
                    directoriesToExcludeFromCopyToIntermediateDir,
                    directoriesToExcludeFromCopyToBuildOutputDir);
                timedEvent.SetProperties(toolsToVersion);
            }

            if (this.checkers != null && checkerMessageSink != null && this.cliOptions.EnableCheckers)
            {
                try
                {
                    this.logger.LogDebug("Running checkers");
                    this.RunCheckers(context, toolsToVersion, checkerMessageSink);
                }
                catch (Exception exc)
                {
                    this.logger.LogError(exc, "Exception caught while running checkers");
                }
            }
            else
            {
                this.logger.LogInformation(
                    "Not running checkers - condition evaluates to " +
                    "({checkersNotNull} && {sinkNotNull} && {enableCheckers})",
                    this.checkers != null,
                    checkerMessageSink != null,
                    this.cliOptions.EnableCheckers);
            }

            if (buildScriptSnippets != null)
            {
                foreach (var snippet in buildScriptSnippets)
                {
                    if (snippet.IsFullScript)
                    {
                        script = snippet.BashBuildScriptSnippet;
                        return;
                    }
                }
            }

            if (buildScriptSnippets.Any())
            {
                // By default exclude these irrespective of platform
                directoriesToExcludeFromCopyToIntermediateDir.Add(".git");
                directoriesToExcludeFromCopyToBuildOutputDir.Add(".git");

                script = this.BuildScriptFromSnippets(
                    context,
                    installationScript,
                    buildScriptSnippets,
                    new ReadOnlyDictionary<string, string>(toolsToVersion),
                    directoriesToExcludeFromCopyToIntermediateDir,
                    directoriesToExcludeFromCopyToBuildOutputDir,
                    detectionResults);
            }
            else
            {
                // TODO: Should an UnsupportedPlatformException be thrown here?
                // Seeing as the issue was that platforms were IDENTIFIED, but no build snippets were emitted from them
                throw new UnsupportedPlatformException(Labels.UnableToDetectPlatformMessage);
            }
        }

        private void RunCheckers(
            BuildScriptGeneratorContext ctx,
            IDictionary<string, string> tools,
            [NotNull] List<ICheckerMessage> checkerMessageSink)
        {
            var checkers = this.checkers.WhereApplicable(tools).ToArray();

            this.logger.LogInformation(
                "Running {checkerCount} applicable checkers for {toolCount} tools: {toolNames}",
                checkers.Length,
                tools.Keys.Count,
                string.Join(',', tools.Keys));

            using (var timedEvent = this.telemetryClient.LogTimedEvent("RunCheckers"))
            {
                var repoMessages = checkers.SelectMany(checker => checker.CheckSourceRepo(ctx.SourceRepo));
                checkerMessageSink.AddRange(repoMessages);

                var toolMessages = checkers.SelectMany(checker => checker.CheckToolVersions(tools));
                checkerMessageSink.AddRange(toolMessages);

                timedEvent.AddProperty("repoMsgCount", repoMessages.Count().ToString());
                timedEvent.AddProperty("toolMsgCount", toolMessages.Count().ToString());

                timedEvent.AddProperty(
                    "checkersApplied",
                    string.Join(',', checkers.Select(checker => checker.GetType().Name)));
            }
        }

        private IList<BuildScriptSnippet> GetBuildSnippets(
            BuildScriptGeneratorContext context,
            IEnumerable<PlatformDetectorResult> detectionResults,
            bool runDetection,
            [CanBeNull] List<string> directoriesToExcludeFromCopyToIntermediateDir,
            [CanBeNull] List<string> directoriesToExcludeFromCopyToBuildOutputDir)
        {
            var snippets = new List<BuildScriptSnippet>();

            IDictionary<IProgrammingPlatform, PlatformDetectorResult> platformsToUse;
            if (runDetection)
            {
                platformsToUse = this.compatiblePlatformDetector.GetCompatiblePlatforms(context);
            }
            else
            {
                platformsToUse = this.compatiblePlatformDetector.GetCompatiblePlatforms(context, detectionResults);
            }

            foreach (var platformAndDetectorResult in platformsToUse)
            {
                var (platform, detectorResult) = platformAndDetectorResult;

                if (directoriesToExcludeFromCopyToIntermediateDir != null)
                {
                    var excludedDirs = platform.GetDirectoriesToExcludeFromCopyToIntermediateDir(context);
                    if (excludedDirs.Any())
                    {
                        directoriesToExcludeFromCopyToIntermediateDir.AddRange(excludedDirs);
                    }
                }

                if (directoriesToExcludeFromCopyToBuildOutputDir != null)
                {
                    var excludedDirs = platform.GetDirectoriesToExcludeFromCopyToBuildOutputDir(context);
                    if (excludedDirs.Any())
                    {
                        directoriesToExcludeFromCopyToBuildOutputDir.AddRange(excludedDirs);
                    }
                }

                string cleanOrNot = platform.IsCleanRepo(context.SourceRepo) ? "clean" : "not clean";
                this.logger.LogDebug($"Repo is {cleanOrNot} for {platform.Name}");

                var snippet = platform.GenerateBashBuildScriptSnippet(context, detectorResult);
                if (snippet != null)
                {
                    this.logger.LogDebug(
                        "Platform {platformName} with version {platformVersion} was used.",
                        platform.Name,
                        detectorResult.PlatformVersion);
                    snippets.Add(snippet);
                }
                else
                {
                    this.logger.LogWarning(
                        "{platformType}.GenerateBashBuildScriptSnippet() returned null",
                        platform.GetType());
                }
            }

            return snippets;
        }

        private void LogScriptIfGiven(string type, string scriptPath)
        {
            if (!string.IsNullOrWhiteSpace(scriptPath))
            {
                this.logger.LogInformation("Using {type} script", type);
            }
        }

        /// <summary>
        /// Builds the full build script from the list of snippets for each platform.
        /// </summary>
        /// <returns>Finalized build script as a string.</returns>
        private string BuildScriptFromSnippets(
            BuildScriptGeneratorContext context,
            string installationScript,
            IList<BuildScriptSnippet> buildScriptSnippets,
            IDictionary<string, string> toolsToVersion,
            List<string> directoriesToExcludeFromCopyToIntermediateDir,
            List<string> directoriesToExcludeFromCopyToBuildOutputDir,
            IEnumerable<PlatformDetectorResult> detectionResults)
        {
            string script;
            string benvArgs = StringExtensions.JoinKeyValuePairs(toolsToVersion);
            benvArgs = $"{benvArgs} {Constants.BenvDynamicInstallRootDirKey}=\"{this.cliOptions.DynamicInstallRootDir}\"";

            Dictionary<string, string> buildProperties = buildScriptSnippets
                .Where(s => s.BuildProperties != null)
                .SelectMany(s => s.BuildProperties)
                .ToDictionary(p => p.Key, p => p.Value);
            buildProperties[ManifestFilePropertyKeys.OperationId] = context.OperationId;

            var sourceDirInBuildContainer = this.cliOptions.SourceDir;
            if (!string.IsNullOrEmpty(this.cliOptions.IntermediateDir))
            {
                sourceDirInBuildContainer = this.cliOptions.IntermediateDir;
            }

            buildProperties[ManifestFilePropertyKeys.SourceDirectoryInBuildContainer] = sourceDirInBuildContainer;

            var allPlatformNames = detectionResults
                .Where(s => s.Platform != null)
                .Select(s => s.Platform)
                .ToList();

            foreach (var eachPlatformName in allPlatformNames)
            {
                this.logger.LogInformation($"Build Property Key:{ManifestFilePropertyKeys.PlatformName} value: {eachPlatformName} is written into manifest");
                if (buildProperties.ContainsKey(ManifestFilePropertyKeys.PlatformName))
                {
                    var previousValue = buildProperties[ManifestFilePropertyKeys.PlatformName];
                    buildProperties[ManifestFilePropertyKeys.PlatformName]
                    = string.Join(
                        ",",
                        previousValue,
                        eachPlatformName);
                }
                else
                {
                    buildProperties[ManifestFilePropertyKeys.PlatformName] = eachPlatformName;
                }
            }

            buildProperties[ManifestFilePropertyKeys.CompressDestinationDir] =
                this.cliOptions.CompressDestinationDir.ToString().ToLower();

            // Process the appsvc.yaml file
            this.ProcessAppSvcYamlFile(context);

            // Process the oryx-config.yaml file
            var extensibleConfiguration = this.ProcessExtensibleConfigurationFile(context);

            (var preBuildCommand, var postBuildCommand) = PreAndPostBuildCommandHelper.GetPreAndPostBuildCommands(
                context.SourceRepo,
                this.cliOptions);

            var outputIsSubDirOfSourceDir = false;
            if (!string.IsNullOrEmpty(this.cliOptions.DestinationDir))
            {
                outputIsSubDirOfSourceDir = DirectoryHelper.IsSubDirectory(
                    this.cliOptions.DestinationDir,
                    this.cliOptions.SourceDir);
            }

            // Copy the source content to destination only if all the platforms involved in generating the build script
            // say yes.
            var copySourceDirectoryContentToDestinationDirectory = buildScriptSnippets.All(
                snippet => snippet.CopySourceDirectoryContentToDestinationDirectory);

            var buildScriptProps = new BaseBashBuildScriptProperties()
            {
                OsPackagesToInstall = this.cliOptions.RequiredOsPackages ?? Array.Empty<string>(),
                BuildScriptSnippets = buildScriptSnippets.Select(s => s.BashBuildScriptSnippet),
                BenvArgs = benvArgs,
                PreBuildCommand = preBuildCommand,
                PostBuildCommand = postBuildCommand,
                DirectoriesToExcludeFromCopyToIntermediateDir = directoriesToExcludeFromCopyToIntermediateDir,
                DirectoriesToExcludeFromCopyToBuildOutputDir = directoriesToExcludeFromCopyToBuildOutputDir,
                ManifestFileName = FilePaths.BuildManifestFileName,
                ManifestDir = context.ManifestDir,
                BuildCommandsFileName = context.BuildCommandsFileName,
                BuildProperties = buildProperties,
                BenvPath = FilePaths.Benv,
                LoggerPath = FilePaths.Logger,
                PlatformInstallationScript = installationScript,
                OutputDirectoryIsNested = outputIsSubDirOfSourceDir,
                CopySourceDirectoryContentToDestinationDirectory = copySourceDirectoryContentToDestinationDirectory,
                CompressDestinationDir = this.cliOptions.CompressDestinationDir,
                ExtensibleConfigurationCommands = extensibleConfiguration,
            };

            this.LogScriptIfGiven("pre-build", buildScriptProps.PreBuildCommand);
            this.LogScriptIfGiven("post-build", buildScriptProps.PostBuildCommand);

            script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.BaseBashScript,
                buildScriptProps,
                this.logger,
                this.telemetryClient);
            return script;
        }

        /// <summary>
        /// Checks for the <see cref="FilePaths.AppSvcFileName"/> file in the source repository, and if it exists,
        /// parses the YAML file for commands specified by the user to be ran during different periods of the build.
        /// </summary>
        /// <param name="context"><see cref="BuildScriptGeneratorContext"/> object containing information regarding
        /// the user's provided source repository.</param>
        private void ProcessAppSvcYamlFile(BuildScriptGeneratorContext context)
        {
            // Workaround for bug in TestSourceRepo class in validation tests
            // Should be using context.SourceRepo.FileExists
            string filePathForAppYaml = Path.Combine(context.SourceRepo.RootPath, FilePaths.AppSvcFileName);

            this.logger.LogDebug($"Path to {FilePaths.AppSvcFileName}: '{filePathForAppYaml}'");

            // Override the prebuild and postbuild commands if BuildConfigurationFile exists
            if (File.Exists(filePathForAppYaml))
            {
                this.logger.LogDebug("Found BuildConfigurationFile");
                this.writer.WriteLine(Environment.NewLine + "Found BuildConfigurationFile");
                try
                {
                    BuildConfigurationFile buildConfigFile = BuildConfigurationFile.Create(context.SourceRepo.ReadFile(FilePaths.AppSvcFileName));
                    if (!string.IsNullOrEmpty(buildConfigFile.Prebuild))
                    {
                        this.cliOptions.PreBuildCommand = buildConfigFile.Prebuild.Replace("\r\n", ";").Replace("\n", ";");
                        this.cliOptions.PreBuildScriptPath = null;
                        this.logger.LogDebug("Overriding the pre-build commands with the BuildConfigurationFile section");
                        this.logger.LogDebug(this.cliOptions.PreBuildCommand.ToString());
                        this.writer.WriteLine("Overriding the pre-build commands with the BuildConfigurationFile section");
                        this.writer.WriteLine("\t" + this.cliOptions.PreBuildCommand.ToString());
                    }

                    if (!string.IsNullOrEmpty(buildConfigFile.Postbuild))
                    {
                        this.cliOptions.PostBuildCommand = buildConfigFile.Postbuild.Replace("\r\n", ";").Replace("\n", ";");
                        this.cliOptions.PostBuildScriptPath = null;
                        this.logger.LogDebug("Overriding the post-build commands with the BuildConfigurationFile section");
                        this.logger.LogDebug(this.cliOptions.PostBuildCommand.ToString());
                        this.writer.WriteLine("Overriding the post-build commands with the BuildConfigurationFile section");
                        this.writer.WriteLine("\t" + this.cliOptions.PostBuildCommand.ToString());
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning("Invalid BuildConfigurationFile " + ex.ToString());
                    this.writer.WriteLine($"{Environment.NewLine}\"{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss")}\" | WARNING | Invalid BuildConfigurationFile | Exit Code: 1 | Please review your {FilePaths.AppSvcFileName} | {Constants.BuildConfigurationFileHelp}");
                    this.writer.WriteLine($"The following is the structure of a valid {FilePaths.AppSvcFileName}:");
                    this.writer.WriteLine("-------------------------------------------");
                    this.writer.WriteLine("version: 1");
                    this.writer.WriteLine("pre-build: apt-get install xyz");
                    this.writer.WriteLine("post-build: |");
                    this.writer.WriteLine("  python manage.py makemigrations");
                    this.writer.WriteLine("  python manage.py migrate");
                    this.writer.WriteLine("-------------------------------------------");
                }
            }
            else
            {
                this.logger.LogDebug($"No {FilePaths.AppSvcFileName} found");
            }
        }

        /// <summary>
        /// Checks for the <see cref="FilePaths.ExtensibleConfigurationFileName"/> file in the source repository, and
        /// if it exists, parses the YAML file for extensibility endpoints defined by the user that will be converted
        /// to runnable steps during the build.
        /// </summary>
        /// <param name="context"><see cref="BuildScriptGeneratorContext"/> object containing information regarding
        /// the user's provided source repository.</param>
        private string ProcessExtensibleConfigurationFile(BuildScriptGeneratorContext context)
        {
            var filePath = Path.Combine(context.SourceRepo.RootPath, FilePaths.ExtensibleConfigurationFileName);
            this.logger.LogDebug($"Checking for path to {FilePaths.ExtensibleConfigurationFileName}: '{filePath}'");
            this.writer.WriteLine($"Checking for path to {FilePaths.ExtensibleConfigurationFileName}: '{filePath}'");

            if (File.Exists(filePath))
            {
                this.logger.LogDebug("Found extensible configuration file.");
                this.writer.WriteLine($"{Environment.NewLine}Found extensible configuration file, {filePath}");
                ExtensibleConfigurationFile config = null;
                try
                {
                    config = ExtensibleConfigurationFile.Create(context.SourceRepo.ReadFile(FilePaths.ExtensibleConfigurationFileName));
                    return config.GetBuildScriptSnippet();
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning($"Error thrown when processing extensible configuration file: {ex}");
                    this.writer.WriteLine($"Error thrown when processing extensible configuration file: {ex}");
                    if (config != null)
                    {
                        var warningMessage = config.GetWarningMessage();
                        this.writer.WriteLine(warningMessage);
                    }
                }
            }
            else
            {
                this.logger.LogDebug($"No {FilePaths.ExtensibleConfigurationFileName} found");
            }

            return string.Empty;
        }
    }
}
