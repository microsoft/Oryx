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
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Finds and resolves scripts generators based on user input and invokes one of them to generate a script.
    /// </summary>
    internal class DefaultBuildScriptGenerator : IBuildScriptGenerator
    {
        private readonly IEnumerable<IProgrammingPlatform> _programmingPlatforms;
        private readonly IEnvironmentSettingsProvider _environmentSettingsProvider;
        private readonly IEnumerable<IChecker> _checkers;
        private readonly ILogger<DefaultBuildScriptGenerator> _logger;

        public DefaultBuildScriptGenerator(
            IEnumerable<IProgrammingPlatform> programmingPlatforms,
            IEnvironmentSettingsProvider environmentSettingsProvider,
            IEnumerable<IChecker> checkers,
            ILogger<DefaultBuildScriptGenerator> logger)
        {
            _programmingPlatforms = programmingPlatforms;
            _environmentSettingsProvider = environmentSettingsProvider;
            _logger = logger;
            _checkers = checkers;
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
                LogAndThrowNoPlatformFound(context);
            }
        }

        public IList<Tuple<IProgrammingPlatform, string>> GetCompatiblePlatforms(BuildScriptGeneratorContext ctx)
        {
            var resultPlatforms = new List<Tuple<IProgrammingPlatform, string>>();

            var enabledPlatforms = _programmingPlatforms.Where(p =>
            {
                if (!p.IsEnabled(ctx))
                {
                    _logger.LogDebug("{platformName} has been disabled", p.Name);
                    return false;
                }
                return true;
            });

            // If a user supplied the language explicitly, check if the platform is enabled for that
            IProgrammingPlatform userSuppliedPlatform = null;
            string platformVersion = null;
            if (!string.IsNullOrEmpty(ctx.Language))
            {
                var selectedPlatform = enabledPlatforms
                    .Where(p => ctx.Language.EqualsIgnoreCase(p.Name))
                    .FirstOrDefault();

                if (selectedPlatform == null)
                {
                    ThrowInvalidLanguageProvided(ctx);
                }

                userSuppliedPlatform = selectedPlatform;

                platformVersion = ctx.LanguageVersion;
                if (string.IsNullOrEmpty(platformVersion))
                {
                    var detectionResult = userSuppliedPlatform.Detect(ctx.SourceRepo);
                    if (detectionResult == null || string.IsNullOrEmpty(detectionResult.LanguageVersion))
                    {
                        throw new UnsupportedVersionException(
                            $"Couldn't detect a version for the platform '{userSuppliedPlatform.Name}' in the repo.");
                    }

                    platformVersion = detectionResult.LanguageVersion;
                }

                resultPlatforms.Add(Tuple.Create(userSuppliedPlatform, platformVersion));

                // if the user explicitly supplied a platform and if that platform does not want to be part of
                // multi-platform builds, then short-circuit immediately ignoring going through other platforms
                if (!IsEnabledForMultiPlatformBuild(userSuppliedPlatform, ctx))
                {
                    return resultPlatforms;
                }
            }

            // Ignore processing the same platform again
            if (userSuppliedPlatform != null)
            {
                enabledPlatforms = enabledPlatforms.Where(p => !ReferenceEquals(p, userSuppliedPlatform));
            }

            foreach (var platform in enabledPlatforms)
            {
                string targetVersionSpec = null;

                _logger.LogDebug("Detecting platform using {platformName}", platform.Name);
                var detectionResult = platform.Detect(ctx.SourceRepo);
                if (detectionResult != null)
                {
                    _logger.LogDebug(
                        "Detected {platformName} version {platformVersion} for app in repo",
                        platform.Name,
                        detectionResult.LanguageVersion);

                    targetVersionSpec = detectionResult.LanguageVersion;
                    if (string.IsNullOrEmpty(targetVersionSpec))
                    {
                        throw new UnsupportedVersionException(
                            $"Couldn't detect a version for the platform '{platform.Name}' in the repo.");
                    }

                    resultPlatforms.Add(Tuple.Create(platform, targetVersionSpec));

                    if (!IsEnabledForMultiPlatformBuild(platform, ctx))
                    {
                        return resultPlatforms;
                    }
                }
            }

            return resultPlatforms;
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
            foreach (Tuple<IProgrammingPlatform, string> platformAndVersion in platformsToUse)
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
                    _logger.LogDebug("Platform {platformType} was used", platform.GetType());
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

        private void ThrowInvalidLanguageProvided(BuildScriptGeneratorContext context)
        {
            var languages = _programmingPlatforms.Select(sg => sg.Name);
            var exc = new UnsupportedLanguageException($"'{context.Language}' platform is not supported. " +
                $"Supported platforms are: {string.Join(", ", languages)}");
            _logger.LogError(exc, "Exception caught");
            throw exc;
        }

        private void LogScriptIfGiven(string type, string scriptPath)
        {
            if (!string.IsNullOrWhiteSpace(scriptPath))
            {
                _logger.LogInformation("Using {type} script from {scriptPath}", type, scriptPath);
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
                BuildScriptSnippets = snippets.Select(s => s.BashBuildScriptSnippet),
                BenvArgs = benvArgs,
                PreBuildCommand = preBuildCommand,
                PostBuildCommand = postBuildCommand,
                DirectoriesToExcludeFromCopyToIntermediateDir = directoriesToExcludeFromCopyToIntermediateDir,
                DirectoriesToExcludeFromCopyToBuildOutputDir = directoriesToExcludeFromCopyToBuildOutputDir,
                ManifestFileName = FilePaths.BuildManifestFileName,
                BuildProperties = buildProperties
            };

            LogScriptIfGiven("pre-build", buildScriptProps.PreBuildCommand);
            LogScriptIfGiven("post-build", buildScriptProps.PostBuildCommand);

            script = TemplateHelpers.Render(
                TemplateHelpers.TemplateResource.BaseBashScript,
                buildScriptProps,
                _logger);
            return script;
        }

        /// <summary>
        /// Handles the error when no platform was found, logging information about the repo.
        /// </summary>
        private void LogAndThrowNoPlatformFound(BuildScriptGeneratorContext context)
        {
            try
            {
                var directoryStructureData = OryxDirectoryStructureHelper.GetDirectoryStructure(
                    context.SourceRepo.RootPath);
                _logger.LogTrace(
                    "logDirectoryStructure",
                    new Dictionary<string, string> { { "directoryStructure", directoryStructureData } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught");
            }
            finally
            {
                throw new UnsupportedLanguageException("Could not detect the language from repo.");
            }
        }

        /// <summary>
        /// Gets a matching version for the platform given a version in SemVer format.
        /// If the given version is not supported, an exception is thrown.
        /// </summary>
        /// <returns>The maximum version that satisfies the requested version spec.</returns>
        private string GetMatchingTargetVersion(IProgrammingPlatform platform, string targetVersionSpec)
        {
            string targetVersion;
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
               targetVersionSpec,
               platform.SupportedVersions);

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exc = new UnsupportedVersionException(platform.Name, targetVersionSpec, platform.SupportedVersions);
                _logger.LogError(exc, "Exception caught");
                throw exc;
            }
            else
            {
                targetVersion = maxSatisfyingVersion;
            }

            return targetVersion;
        }

        private bool IsEnabledForMultiPlatformBuild(IProgrammingPlatform platform, BuildScriptGeneratorContext context)
        {
            if (context.DisableMultiPlatformBuild)
            {
                return false;
            }

            return platform.IsEnabledForMultiPlatformBuild(context);
        }
    }
}