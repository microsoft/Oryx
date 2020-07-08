// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Detector;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestPlatformDetectorUsingPlatformName : IPlatformDetector
    {
        private readonly string _platformName;
        private readonly string _platformVersion;

        public TestPlatformDetectorUsingPlatformName(string detectedPlatformName, string detectedPlatformVersion)
        {
            _platformName = detectedPlatformName;
            _platformVersion = detectedPlatformVersion;
        }

        public bool DetectInvoked { get; private set; }

        public string PlatformName => _platformName;

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            DetectInvoked = true;

            if (!string.IsNullOrEmpty(_platformName))
            {
                return new PlatformDetectorResult
                {
                    Platform = _platformName,
                    PlatformVersion = _platformVersion,
                };
            }
            return null;
        }
    }

    public class TestPlatformDetectorSimpleMatch : IPlatformDetector
    {
        private readonly string _platformVersion;
        private bool _shouldMatch;
        private readonly string _platformName;

        public TestPlatformDetectorSimpleMatch(
            bool shouldMatch,
            string platformName = "universe",
            string platformVersion = "42")
        {
            _shouldMatch = shouldMatch;
            _platformName = platformName;
            _platformVersion = platformVersion;
        }

        public bool DetectInvoked { get; private set; }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            DetectInvoked = true;

            if (_shouldMatch)
            {
                return new PlatformDetectorResult
                {
                    Platform = _platformName,
                    PlatformVersion = _platformVersion
                };
            }
            else
            {
                return null;
            }
        }
    }
}
