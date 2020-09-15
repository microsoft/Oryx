// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Ruby;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Ruby;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Ruby
{
    internal class TestRubyPlatformDetector : IRubyPlatformDetector
    {
        private readonly string _detectedVersion;

        public TestRubyPlatformDetector(string detectedVersion = null)
        {
            _detectedVersion = detectedVersion;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            return new RubyPlatformDetectorResult
            {
                Platform = RubyConstants.PlatformName,
                PlatformVersion = _detectedVersion,
            };
        }
    }
}
