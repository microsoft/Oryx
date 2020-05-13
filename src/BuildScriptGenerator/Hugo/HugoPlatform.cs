// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Hugo
{
    class HugoPlatform : IProgrammingPlatform
    {
        private readonly IEnvironment _environment;
        private readonly ILogger<HugoPlatform> _logger;

        public HugoPlatform(IEnvironment environment, ILogger<HugoPlatform> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public string Name => HugoConstants.Name;

        public IEnumerable<string> SupportedVersions => new[] { HugoConstants.Version };

        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            var isHugoApp = StaticSiteGeneratorHelper.IsHugoApp(context.SourceRepo, _environment);
            if (isHugoApp)
            {
                return new PlatformDetectorResult
                {
                    Platform = HugoConstants.Name,
                    PlatformVersion = HugoConstants.Version,
                };
            }

            return null;
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            string script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.HugoSnippet,
                model: null,
                _logger);

            return new BuildScriptSnippet
            {
                BashBuildScriptSnippet = script,
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
        }

        public void SetVersion(BuildScriptGeneratorContext context, string version)
        {
        }
    }
}
