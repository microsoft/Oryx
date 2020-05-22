// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Hugo
{
    class HugoPlatform : IProgrammingPlatform
    {
        private readonly IEnvironment _environment;
        private readonly ILogger<HugoPlatform> _logger;
        private readonly HugoPlatformInstaller _platformInstaller;
        private readonly BuildScriptGeneratorOptions _commonOptions;

        public HugoPlatform(
            IEnvironment environment,
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILogger<HugoPlatform> logger,
            HugoPlatformInstaller platformInstaller)
        {
            _environment = environment;
            _logger = logger;
            _platformInstaller = platformInstaller;
            _commonOptions = commonOptions.Value;
        }

        public string Name => HugoConstants.PlatformName;

        public IEnumerable<string> SupportedVersions => new[] { HugoConstants.Version };

        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            var isHugoApp = StaticSiteGeneratorHelper.IsHugoApp(context.SourceRepo, _environment);
            if (isHugoApp)
            {
                return new PlatformDetectorResult
                {
                    Platform = HugoConstants.PlatformName,
                    PlatformVersion = HugoConstants.Version,
                };
            }

            return null;
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext context)
        {
            string installationScriptSnippet = null;
            if (_commonOptions.EnableDynamicInstall)
            {
                _logger.LogDebug("Dynamic install is enabled.");

                if (_platformInstaller.IsVersionAlreadyInstalled(context.ResolvedHugoVersion))
                {
                    _logger.LogDebug(
                       "Hugo version {version} is already installed. So skipping installing it again.",
                       context.ResolvedHugoVersion);
                }
                else
                {
                    _logger.LogDebug(
                        "Hugo version {version} is not installed. " +
                        "So generating an installation script snippet for it.",
                        context.ResolvedHugoVersion);

                    installationScriptSnippet = _platformInstaller.GetInstallerScriptSnippet(
                        context.ResolvedHugoVersion);
                }
            }
            else
            {
                _logger.LogDebug("Dynamic install not enabled.");
            }

            var manifestFileProperties = new Dictionary<string, string>();
            manifestFileProperties[ManifestFilePropertyKeys.HugoVersion] = context.ResolvedHugoVersion;

            string script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.HugoSnippet,
                model: null,
                _logger);

            return new BuildScriptSnippet
            {
                BashBuildScriptSnippet = script,
                PlatformInstallationScriptSnippet = installationScriptSnippet,
                BuildProperties = manifestFileProperties,
            };
        }

        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
        {
            return null;
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return Array.Empty<string>();
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return Array.Empty<string>();
        }

        public string GetMaxSatisfyingVersionAndVerify(string version)
        {
            return version;
        }

        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        public bool IsEnabled(RepositoryContext ctx)
        {
            return true;
        }

        public bool IsEnabledForMultiPlatformBuild(RepositoryContext ctx)
        {
            return true;
        }

        public void SetRequiredTools(
            ISourceRepo sourceRepo,
            string targetPlatformVersion,
            [NotNull] IDictionary<string, string> toolsToVersion)
        {
            if (!string.IsNullOrWhiteSpace(targetPlatformVersion))
            {
                toolsToVersion[ToolNameConstants.HugoName] = targetPlatformVersion;
            }
        }

        public void SetVersion(BuildScriptGeneratorContext context, string version)
        {
            context.ResolvedHugoVersion = version;
        }
    }
}
