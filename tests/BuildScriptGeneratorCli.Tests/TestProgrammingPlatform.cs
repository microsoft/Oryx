// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    internal class TestProgrammingPlatform : IProgrammingPlatform
    {
        private const string DefaultScriptContent = "#!/bin/bash\necho Hello World\n";
        private readonly string _scriptContent;

        public TestProgrammingPlatform()
            : this(scriptContent: null)
        {
        }

        public TestProgrammingPlatform(string scriptContent)
        {
            _scriptContent = scriptContent;
        }

        public string Name => "test";

        public IEnumerable<string> SupportedLanguageVersions => new[] { "1.0.0" };

        public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
        {
            return new LanguageDetectorResult
            {
                Language = Name,
                LanguageVersion = SupportedLanguageVersions.First()
            };
        }

        public BuildScriptSnippet GenerateBashBuildScriptSnippet(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            string script = string.IsNullOrEmpty(_scriptContent) ? DefaultScriptContent : _scriptContent;
            return new BuildScriptSnippet { BashBuildScriptSnippet = script };
        }

        public string GenerateBashRunScript(RunScriptGeneratorOptions runScriptGeneratorOptions)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return Array.Empty<string>();
        }

        public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(
            BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return Array.Empty<string>();
        }

        public bool IsCleanRepo(ISourceRepo repo)
        {
            return true;
        }

        public bool IsEnabled(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            return true;
        }

        public void SetRequiredTools(
            ISourceRepo sourceRepo,
            string targetPlatformVersion,
            IDictionary<string, string> toolsToVersion)
        {
        }

        public void SetVersion(BuildScriptGeneratorContext context, string version)
        {
        }

        public bool IsEnabledForMultiPlatformBuild(BuildScriptGeneratorContext scriptGeneratorContext)
        {
            throw new NotImplementedException();
        }
    }
}