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
            SupportedVersions = languageVersions;
            _canGenerateScript = canGenerateScript;
            _scriptContent = scriptContent;
            _detector = detector;
            _enabled = enabled;
            _platformIsEnabledForMultiPlatformBuild = platformIsEnabledForMultiPlatformBuild;
        }

        public string Name { get; }

        public IEnumerable<string> SupportedVersions { get; }

        public LanguageDetectorResult Detect(RepositoryContext context)
        {
            return _detector?.Detect(context);
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext _)
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

        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        public bool IsEnabled(RepositoryContext ctx)
        {
            return _enabled;
        }

        public void SetRequiredTools(
            ISourceRepo sourceRepo,
            string targetPlatformVersion,
            IDictionary<string, string> toolsToVersion)
        {
            toolsToVersion.Add(Name, targetPlatformVersion);
        }

        public void SetVersion(BuildScriptGeneratorContext context, string version)
        {
        }

        public bool IsEnabledForMultiPlatformBuild(RepositoryContext scriptGeneratorContext)
        {
            return _platformIsEnabledForMultiPlatformBuild;
        }
    }
}