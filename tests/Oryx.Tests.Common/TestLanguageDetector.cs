// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestLanguageDetectorUsingLangName : ILanguageDetector
    {
        private readonly string _languageName;
        private readonly string _languageVersion;

        public TestLanguageDetectorUsingLangName(string detectedLanguageName, string detectedLanguageVersion)
        {
            _languageName = detectedLanguageName;
            _languageVersion = detectedLanguageVersion;
        }

        public bool DetectInvoked { get; private set; }

        public LanguageDetectorResult Detect(RepositoryContext context)
        {
            DetectInvoked = true;

            if (!string.IsNullOrEmpty(_languageName))
            {
                return new LanguageDetectorResult
                {
                    Language = _languageName,
                    LanguageVersion = _languageVersion,
                };
            }
            return null;
        }
    }

    public class TestLanguageDetectorSimpleMatch : ILanguageDetector
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

        public LanguageDetectorResult Detect(RepositoryContext context)
        {
            DetectInvoked = true;

            if (_shouldMatch)
            {
                return new LanguageDetectorResult
                {
                    Language = _language,
                    LanguageVersion = _languageVersion
                };
            }
            else
            {
                return null;
            }
        }
    }
}
