// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Detector;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestProgrammingPlatform : IProgrammingPlatform
    {
        private readonly bool? _canGenerateScript;
        private readonly string _scriptContent;
        private readonly string _installationScriptContent;
        private readonly IPlatformDetector _detector;
        private bool _enabled;
        private bool _platformIsEnabledForMultiPlatformBuild;

        public TestProgrammingPlatform(
            string platformName,
            string[] platformVersions,
            bool? canGenerateScript = null,
            string scriptContent = null,
            string installationScriptContent = null,
            IPlatformDetector detector = null,
            bool enabled = true,
            bool platformIsEnabledForMultiPlatformBuild = true)
        {
            Name = platformName;
            SupportedVersions = platformVersions;
            _canGenerateScript = canGenerateScript;
            _scriptContent = scriptContent;
            _installationScriptContent = installationScriptContent;
            _detector = detector;
            _enabled = enabled;
            _platformIsEnabledForMultiPlatformBuild = platformIsEnabledForMultiPlatformBuild;
        }

        public string Name { get; }

        public IEnumerable<string> SupportedVersions { get; }

        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            return _detector?.Detect(new DetectorContext
            {
                SourceRepo = new Detector.LocalSourceRepo(context.SourceRepo?.RootPath),
            });
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(
            BuildScriptGeneratorContext context,
            PlatformDetectorResult detectorResult)
        {
            if (_canGenerateScript == true)
            {
                return new BuildScriptSnippet { BashBuildScriptSnippet = _scriptContent };
            }

            return null;
        }

        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions _)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(BuildScriptGeneratorContext _)
        {
            return Array.Empty<string>();
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(BuildScriptGeneratorContext _)
        {
            return Array.Empty<string>();
        }

        public bool IsCleanRepo(BuildScriptGenerator.ISourceRepo repo)
        {
            return true;
        }

        public bool IsEnabled(RepositoryContext ctx)
        {
            return _enabled;
        }

        public bool IsEnabledForMultiPlatformBuild(RepositoryContext scriptGeneratorContext)
        {
            return _platformIsEnabledForMultiPlatformBuild;
        }

        public string GetMaxSatisfyingVersionAndVerify(string version)
        {
            return version;
        }

        public string GetInstallerScriptSnippet(
            BuildScriptGeneratorContext context,
            PlatformDetectorResult detectorResult)
        {
            return _installationScriptContent;
        }

        public void ResolveVersions(RepositoryContext context, PlatformDetectorResult detectorResult)
        {
        }

        public IDictionary<string, string> GetToolsToBeSetInPath(
            RepositoryContext context, 
            PlatformDetectorResult detectorResult)
        {
            return new Dictionary<string, string>
            {
                { detectorResult.Platform, detectorResult.PlatformVersion }
            };
        }
    }
}