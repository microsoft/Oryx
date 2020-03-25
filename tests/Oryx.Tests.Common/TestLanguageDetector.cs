// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestPlatformDetectorUsingLangName : IPlatformDetector
    {
        private readonly string _languageName;
        private readonly string _languageVersion;

        public TestPlatformDetectorUsingLangName(string detectedPlatformName, string detectedPlatformVersion)
        {
            _languageName = detectedPlatformName;
            _languageVersion = detectedPlatformVersion;
        }

        public bool DetectInvoked { get; private set; }

        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            DetectInvoked = true;

            if (!string.IsNullOrEmpty(_languageName))
            {
                return new PlatformDetectorResult
                {
                    Platform = _languageName,
                    PlatformVersion = _languageVersion,
                };
            }
            return null;
        }
    }

    public class TestLanguageDetectorSimpleMatch : IPlatformDetector
    {
        private readonly string _languageVersion;
        private bool _shouldMatch;
        private readonly string _language;

        public TestLanguageDetectorSimpleMatch(
            bool shouldMatch,
            string language = "universe",
            string languageVersion = "42")
        {
            _shouldMatch = shouldMatch;
            _language = language;
            _languageVersion = languageVersion;
        }

        public bool DetectInvoked { get; private set; }

        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            DetectInvoked = true;

            if (_shouldMatch)
            {
                return new PlatformDetectorResult
                {
                    Platform = _language,
                    PlatformVersion = _languageVersion
                };
            }
            else
            {
                return null;
            }
        }
    }
}
