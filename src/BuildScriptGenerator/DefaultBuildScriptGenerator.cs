// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Resources;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Common.Extensions;
using Microsoft.Oryx.Detector;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Finds and resolves scripts generators based on user input and invokes one of them to generate a script.
    /// </summary>
    internal class DefaultBuildScriptGenerator : IBuildScriptGenerator
    {
        private readonly IEnumerable<IProgrammingPlatform> _programmingPlatforms;
        private readonly IConfiguration _configuration;
        private readonly BuildScriptGeneratorOptions _cliOptions;
        private readonly IEnvironment _environment;
        private readonly ICompatiblePlatformDetector _platformDetector;
        private readonly IEnumerable<IChecker> _checkers;
        private readonly ILogger<DefaultBuildScriptGenerator> _logger;
        private readonly IStandardOutputWriter _writer;

        public DefaultBuildScriptGenerator(
            IEnumerable<IProgrammingPlatform> programmingPlatforms,
            IConfiguration configuration,
            IOptions<BuildScriptGeneratorOptions> cliOptions,
            ICompatiblePlatformDetector platformDetector,
            IEnumerable<IChecker> checkers,
            ILogger<DefaultBuildScriptGenerator> logger,
            IEnvironment environment,
            IStandardOutputWriter writer)
        {
            _programmingPlatforms = programmingPlatforms;
            _configuration = configuration;
            _cliOptions = cliOptions.Value;
            _environment = environment;
            _platformDetector = platformDetector;
            _logger = logger;
            _checkers = checkers;
            _writer = writer;
            _logger.LogDebug("Available checkers: {checkerCount}", _checkers?.Count() ?? 0);
        }

        public void GenerateBashScript(
            BuildScriptGeneratorContext context,
            out string script,
            List<ICheckerMessage> checkerMessageSink = null)
        {
            script = null;

            // To be populated by GetBuildSnippets
            var toolsToVersion = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            IList<BuildScriptSnippet> buildScriptSnippets;
            var directoriesToExcludeFromCopyToIntermediateDir = new List<string>();
            var directoriesToExcludeFromCopyToBuildOutputDir = new List<string>();

            // Try detecting ALL platforms since in some scenarios this is required.
            // For example, in case of a multi-platform app like ASP.NET Core + NodeJs, we might need to dynamically
            // install both these platforms' sdks before actually using any of their commands. So even though a user
            // of Oryx might explicitly supply the platform of the app as .NET Core, we still need to make sure the
            // build environment is setup with detected platforms' sdks.
            var detectionResults = DetectPlatforms(context);
            var installationScriptSnippets = GetInstallationScriptSnippets(detectionResults, context);

            using (var timedEvent = _logger.LogTimedEvent("GetBuildSnippets"))
            {
                buildScriptSnippets = GetBuildSnippets(
                    context,
                    detectionResults,
                    runDetection: false,
                    toolsToVersion,
                    directoriesToExcludeFromCopyToIntermediateDir,
                    directoriesToExcludeFromCopyToBuildOutputDir);
                timedEvent.SetProperties(toolsToVersion);
            }

            if (_checkers != null && checkerMessageSink != null && _cliOptions.EnableCheckers)
            {
                try
                {
                    _logger.LogDebug("Running checkers");
                    RunCheckers(context, toolsToVersion, checkerMessageSink);
                }
                catch (Exception exc)
                {
                    _logger.LogError(exc, "Exception caught while running checkers");
                }
            }
            else
            {
                _logger.LogInformation("Not running checkers - condition evaluates to " +
                                       "({checkersNotNull} && {sinkNotNull} && {enableCheckers})",
                                       _checkers != null, checkerMessageSink != null, _cliOptions.EnableCheckers);
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

                script = BuildScriptFromSnippets(
                    context,
                    installationScriptSnippets,
                    buildScriptSnippets,
                    new ReadOnlyDictionary<string, string>(toolsToVersion),
                    directoriesToExcludeFromCopyToIntermediateDir,
                    directoriesToExcludeFromCopyToBuildOutputDir);
            }
            else
            {
                // TODO: Should an UnsupportedPlatformException be thrown here?
                // Seeing as the issue was that platforms were IDENTIFIED, but no build snippets were emitted from them
                throw new UnsupportedPlatformException(Labels.UnableToDetectPlatformMessage);
            }
        }

        public IDictionary<string, string> GetRequiredToolVersions(BuildScriptGeneratorContext context)
        {
            var toolsToVersion = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            GetBuildSnippets(
                context,
                detectionResults: null,
                runDetection: true,
                toolsToVersion,
                directoriesToExcludeFromCopyToBuildOutputDir: null,
                directoriesToExcludeFromCopyToIntermediateDir: null);

            return new ReadOnlyDictionary<string, string>(toolsToVersion);
        }

        private void RunCheckers(
            BuildScriptGeneratorContext ctx,
            IDictionary<string, string> tools,
            [NotNull] List<ICheckerMessage> checkerMessageSink)
        {
            var checkers = _checkers.WhereApplicable(tools).ToArray();

            _logger.LogInformation("Running {checkerCount} applicable checkers for {toolCount} tools: {toolNames}",
                checkers.Length, tools.Keys.Count, string.Join(',', tools.Keys));

            using (var timedEvent = _logger.LogTimedEvent("RunCheckers"))
            {
                var repoMessages = checkers.SelectMany(checker => checker.CheckSourceRepo(ctx.SourceRepo));
                checkerMessageSink.AddRange(repoMessages);

                var toolMessages = checkers.SelectMany(checker => checker.CheckToolVersions(tools));
                checkerMessageSink.AddRange(toolMessages);

                timedEvent.AddProperty("repoMsgCount", repoMessages.Count().ToString());
                timedEvent.AddProperty("toolMsgCount", toolMessages.Count().ToString());

                timedEvent.AddProperty("checkersApplied",
                    string.Join(',', checkers.Select(checker => checker.GetType().Name)));
            }
        }

        private IList<BuildScriptSnippet> GetBuildSnippets(
            BuildScriptGeneratorContext context,
            IEnumerable<PlatformDetectorResult> detectionResults,
            bool runDetection,
            Dictionary<string, string> toolsToVersion,
            [CanBeNull] List<string> directoriesToExcludeFromCopyToIntermediateDir,
            [CanBeNull] List<string> directoriesToExcludeFromCopyToBuildOutputDir)
        {
            var snippets = new List<BuildScriptSnippet>();

            IDictionary<IProgrammingPlatform, string> platformsToUse;
            if (runDetection)
            {
                platformsToUse = _platformDetector.GetCompatiblePlatforms(context);
            }
            else
            {
                platformsToUse = _platformDetector.GetCompatiblePlatforms(context, detectionResults);
            }

            foreach (KeyValuePair<IProgrammingPlatform, string> platformAndVersion in platformsToUse)
            {
                var (platform, targetVersionSpec) = platformAndVersion;

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

                string targetVersion = GetMatchingTargetVersion(platform, targetVersionSpec);
                platform.SetVersion(context, targetVersion);

                string cleanOrNot = platform.IsCleanRepo(context.SourceRepo) ? "clean" : "not clean";
                _logger.LogDebug($"Repo is {cleanOrNot} for {platform.Name}");

                var snippet = platform.GenerateBashBuildScriptSnippet(context);
                if (snippet != null)
                {
                    _logger.LogDebug(
                        "Platform {platformName} with version {platformVersion} was used.",
                        platform.Name,
                        targetVersion);
                    snippets.Add(snippet);
                    platform.SetRequiredTools(context.SourceRepo, targetVersion, toolsToVersion);
                }
                else
                {
                    _logger.LogWarning("{platformType}.GenerateBashBuildScriptSnippet() returned null",
                        platform.GetType());
                }
            }

            return snippets;
        }

        private void LogScriptIfGiven(string type, string scriptPath)
        {
            if (!string.IsNullOrWhiteSpace(scriptPath))
            {
                _logger.LogInformation("Using {type} script", type);
            }
        }

        /// <summary>
        /// Builds the full build script from the list of snippets for each platform.
        /// </summary>
        /// <returns>Finalized build script as a string.</returns>
        private string BuildScriptFromSnippets(
            BuildScriptGeneratorContext context,
            IEnumerable<string> installationScriptSnippets,
            IList<BuildScriptSnippet> buildScriptSnippets,
            IDictionary<string, string> toolsToVersion,
            List<string> directoriesToExcludeFromCopyToIntermediateDir,
            List<string> directoriesToExcludeFromCopyToBuildOutputDir)
        {
            string script;
            string benvArgs = StringExtensions.JoinKeyValuePairs(toolsToVersion);

            Dictionary<string, string> buildProperties = buildScriptSnippets
                .Where(s => s.BuildProperties != null)
                .SelectMany(s => s.BuildProperties)
                .ToDictionary(p => p.Key, p => p.Value);
            buildProperties[ManifestFilePropertyKeys.OperationId] = context.OperationId;

            (var preBuildCommand, var postBuildCommand) = PreAndPostBuildCommandHelper.GetPreAndPostBuildCommands(
                context.SourceRepo,
                _cliOptions);

            var outputIsSubDirOfSourceDir = false;
            if (!string.IsNullOrEmpty(_cliOptions.DestinationDir))
            {
                outputIsSubDirOfSourceDir = DirectoryHelper.IsSubDirectory(
                    _cliOptions.DestinationDir,
                    _cliOptions.SourceDir);
            }

            // Copy the source content to destination only if all the platforms involved in generating the build script
            // say yes.
            var copySourceDirectoryContentToDestinationDirectory = buildScriptSnippets.All(
                snippet => snippet.CopySourceDirectoryContentToDestinationDirectory);

            var buildScriptProps = new BaseBashBuildScriptProperties()
            {
                OsPackagesToInstall = _cliOptions.RequiredOsPackages ?? new string[0],
                BuildScriptSnippets = buildScriptSnippets.Select(s => s.BashBuildScriptSnippet),
                BenvArgs = benvArgs,
                PreBuildCommand = preBuildCommand,
                PostBuildCommand = postBuildCommand,
                DirectoriesToExcludeFromCopyToIntermediateDir = directoriesToExcludeFromCopyToIntermediateDir,
                DirectoriesToExcludeFromCopyToBuildOutputDir = directoriesToExcludeFromCopyToBuildOutputDir,
                ManifestFileName = FilePaths.BuildManifestFileName,
                ManifestDir = context.ManifestDir,
                BuildProperties = buildProperties,
                BenvPath = FilePaths.Benv,
                PlatformInstallationScriptSnippets = installationScriptSnippets,
                OutputDirectoryIsNested = outputIsSubDirOfSourceDir,
                CopySourceDirectoryContentToDestinationDirectory = copySourceDirectoryContentToDestinationDirectory,
            };

            LogScriptIfGiven("pre-build", buildScriptProps.PreBuildCommand);
            LogScriptIfGiven("post-build", buildScriptProps.PostBuildCommand);

            script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.BaseBashScript,
                buildScriptProps,
                _logger);
            return script;
        }

        /// <summary>
        /// Gets a matching version for the platform given a version in SemVer format.
        /// If the given version is not supported, an exception is thrown.
        /// </summary>
        /// <returns>The maximum version that satisfies the requested version spec.</returns>
        private string GetMatchingTargetVersion(IProgrammingPlatform platform, string targetVersionSpec)
        {
            return platform.GetMaxSatisfyingVersionAndVerify(targetVersionSpec);
        }

        private IEnumerable<PlatformDetectorResult> DetectPlatforms(BuildScriptGeneratorContext context)
        {
            var detectionResults = new List<PlatformDetectorResult>();

            foreach (var platform in _programmingPlatforms)
            {
                var detectionResult = platform.Detect(context);
                if (detectionResult != null)
                {
                    string resolvedVersion;
                    var version = GetPlatformVersion(platform.Name);
                    if (string.IsNullOrEmpty(version))
                    {
                        resolvedVersion = detectionResult.PlatformVersion;
                    }
                    else
                    {
                        resolvedVersion = platform.GetMaxSatisfyingVersionAndVerify(version);
                    }

                    detectionResult.PlatformVersion = resolvedVersion;
                    detectionResults.Add(detectionResult);
                }
            }

            return detectionResults;
        }

        private IEnumerable<string> GetInstallationScriptSnippets(
            IEnumerable<PlatformDetectorResult> detectionResults,
            BuildScriptGeneratorContext context)
        {
            var installationScriptSnippets = new List<string>();

            foreach (var detectionResult in detectionResults)
            {
                var platform = _programmingPlatforms
                    .Where(p => p.Name.EqualsIgnoreCase(detectionResult.Platform))
                    .First();
                platform.SetVersion(context, detectionResult.PlatformVersion);
                var snippet = platform.GetInstallerScriptSnippet(context);
                installationScriptSnippets.Add(snippet);
            }

            return installationScriptSnippets;
        }

        /// <summary>
        /// Gets the platform version in a hierarchical fasion
        /// 1. --platform nodejs --platform-version 4.0
        /// 2. NODE_VERSION=4.0 from environment variables
        /// 3. NODE_VERSION=4.0 from build.env file
        /// </summary>
        /// <param name="platformName">Platform for which we want to get the version in a hierarchical way.</param>
        /// <returns></returns>
        private string GetPlatformVersion(string platformName)
        {
            platformName = platformName == "nodejs" ? "node" : platformName;

            return _configuration[$"{platformName}_version"];
        }
    }
}