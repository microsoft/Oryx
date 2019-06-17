// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestProgrammingPlatform : IProgrammingPlatform
    {
        private readonly bool? _canGenerateScript;
        private readonly string _scriptContent;
        private readonly ILanguageDetector _detector;
        private bool _enabled;
        private bool _platformIsEnabledForMultiPlatformBuild;

        public TestProgrammingPlatform(
            string languageName,
            string[] languageVersions,
            bool? canGenerateScript = null,
            string scriptContent = null,
            ILanguageDetector detector = null,
            bool enabled = true,
            bool platformIsEnabledForMultiPlatformBuild = true)
        {
            Name = languageName;
            SupportedLanguageVersions = languageVersions;
            _canGenerateScript = canGenerateScript;
            _scriptContent = scriptContent;
            _detector = detector;
            _enabled = enabled;
            _platformIsEnabledForMultiPlatformBuild = platformIsEnabledForMultiPlatformBuild;
        }

        public string Name { get; }

        public IEnumerable<string> SupportedLanguageVersions { get; }

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            return _detector?.Detect(sourceRepo);
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext _)
        {
            if (_canGenerateScript == true)
            {
                return new BuildScriptSnippet { BashBuildScriptSnippet = _scriptContent };
            }

            return null;
        }

        public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
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

        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        public bool IsEnabled(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return _enabled;
        }

        public void SetRequiredTools(
            ISourceRepo sourceRepo,
            string targetPlatformVersion,
            IDictionary<string, string> toolsToVersion)
        {
            toolsToVersion.Add(Name, SupportedLanguageVersions.First());
        }

        public void SetVersion(BuildScriptGeneratorContext context, string version)
        {
        }

        public bool IsEnabledForMultiPlatformBuild(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return _platformIsEnabledForMultiPlatformBuild;
        }
    }
}