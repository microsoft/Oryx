// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Resources;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Finds and resolves scripts generators based on user input and invokes one of them to generate a script.
    /// </summary>
    internal class DefaultBuildScriptGenerator : IBuildScriptGenerator
    {
        private readonly ICompatiblePlatformDetector _platformDetector;
        private readonly IEnvironmentSettingsProvider _environmentSettingsProvider;
        private readonly IEnumerable<IChecker> _checkers;
        private readonly ILogger<DefaultBuildScriptGenerator> _logger;
        private readonly IStandardOutputWriter _writer;

        public DefaultBuildScriptGenerator(
            ICompatiblePlatformDetector platformDetector,
            IEnvironmentSettingsProvider environmentSettingsProvider,
            IEnumerable<IChecker> checkers,
            ILogger<DefaultBuildScriptGenerator> logger,
            IStandardOutputWriter writer)
        {
            _platformDetector = platformDetector;
            _environmentSettingsProvider = environmentSettingsProvider;
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

            IList<BuildScriptSnippet> snippets;
            var directoriesToExcludeFromCopyToIntermediateDir = new List<string>();
            var directoriesToExcludeFromCopyToBuildOutputDir = new List<string>();

            using (var timedEvent = _logger.LogTimedEvent("GetBuildSnippets"))
            {
                snippets = GetBuildSnippets(
                    context,
                    toolsToVersion,
                    directoriesToExcludeFromCopyToIntermediateDir,
                    directoriesToExcludeFromCopyToBuildOutputDir);
                timedEvent.SetProperties(toolsToVersion);
            }

            if (_checkers != null && checkerMessageSink != null && context.EnableCheckers)
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
                                       _checkers != null, checkerMessageSink != null, context.EnableCheckers);
            }

            if (snippets != null)
            {
                foreach (var snippet in snippets)
                {
                    if (snippet.IsFullScript)
                    {
                        script = snippet.BashBuildScriptSnippet;
                        return;
                    }
                }
            }

            if (snippets.Any())
            {
                // By default exclude these irrespective of platform
                directoriesToExcludeFromCopyToIntermediateDir.Add(".git");
                directoriesToExcludeFromCopyToBuildOutputDir.Add(".git");

                script = BuildScriptFromSnippets(
                    context,
                    snippets,
                    new ReadOnlyDictionary<string, string>(toolsToVersion),
                    directoriesToExcludeFromCopyToIntermediateDir,
                    directoriesToExcludeFromCopyToBuildOutputDir);
            }
            else
            {
                // TODO: Should an UnsupportedLanguageException be thrown here?
                // Seeing as the issue was that platforms were IDENTIFIED, but no build snippets were emitted from them
                throw new UnsupportedLanguageException(Labels.UnableToDetectLanguageMessage);
            }
        }

        public IDictionary<IProgrammingPlatform, string> GetCompatiblePlatforms(BuildScriptGeneratorContext ctx)
        {
            return _platformDetector.GetCompatiblePlatforms(ctx, ctx.Language, ctx.LanguageVersion);
        }

        public IDictionary<string, string> GetRequiredToolVersions(BuildScriptGeneratorContext ctx)
        {
            var toolsToVersion = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            GetBuildSnippets(ctx, toolsToVersion, null, null);
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
            Dictionary<string, string> toolsToVersion,
            [CanBeNull] List<string> directoriesToExcludeFromCopyToIntermediateDir,
            [CanBeNull] List<string> directoriesToExcludeFromCopyToBuildOutputDir)
        {
            var snippets = new List<BuildScriptSnippet>();

            var platformsToUse = GetCompatiblePlatforms(context);

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
            IList<BuildScriptSnippet> snippets,
            IDictionary<string, string> toolsToVersion,
            List<string> directoriesToExcludeFromCopyToIntermediateDir,
            List<string> directoriesToExcludeFromCopyToBuildOutputDir)
        {
            string script;
            string benvArgs = StringExtensions.JoinKeyValuePairs(toolsToVersion);
            _environmentSettingsProvider.TryGetAndLoadSettings(out var environmentSettings);

            Dictionary<string, string> buildProperties = snippets
                .Where(s => s.BuildProperties != null)
                .SelectMany(s => s.BuildProperties)
                .ToDictionary(p => p.Key, p => p.Value);
            buildProperties[ManifestFilePropertyKeys.OperationId] = context.OperationId;

            (var preBuildCommand, var postBuildCommand) = PreAndPostBuildCommandHelper.GetPreAndPostBuildCommands(
                context.SourceRepo,
                environmentSettings);

            var buildScriptProps = new BaseBashBuildScriptProperties()
            {
                OsPackagesToInstall = context.RequiredOsPackages ?? new string[0],
                BuildScriptSnippets = snippets.Select(s => s.BashBuildScriptSnippet),
                BenvArgs = benvArgs,
                PreBuildCommand = preBuildCommand,
                PostBuildCommand = postBuildCommand,
                DirectoriesToExcludeFromCopyToIntermediateDir = directoriesToExcludeFromCopyToIntermediateDir,
                DirectoriesToExcludeFromCopyToBuildOutputDir = directoriesToExcludeFromCopyToBuildOutputDir,
                ManifestFileName = FilePaths.BuildManifestFileName,
                ManifestDir = context.ManifestDir,
                BuildProperties = buildProperties,
                BenvPath = FilePaths.Benv,
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
            var matchingRange = SemanticVersionResolver.GetMatchingRange(
                    targetVersionSpec,
                    platform.SupportedVersions);
            if (!matchingRange.Equals(SemanticVersionResolver.NoRangeMatch))
            {
                return matchingRange.ToString();
            }

            var exc = new UnsupportedVersionException(platform.Name, targetVersionSpec, platform.SupportedVersions);
            _logger.LogError(
                exc,
                $"Exception caught, the given version '{targetVersionSpec}' is " +
                $"not supported for platform '{platform.Name}'.");
            throw exc;
        }
    }
}