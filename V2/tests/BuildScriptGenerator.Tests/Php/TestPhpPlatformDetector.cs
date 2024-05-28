// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Php;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Php
{
    internal class TestPhpPlatformDetector : IPhpPlatformDetector
    {
        private readonly string _detectedVersion;

        public TestPhpPlatformDetector(string detectedVersion = null)
        {
            _detectedVersion = detectedVersion;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            return new PhpPlatformDetectorResult
            {
                Platform = PhpConstants.PlatformName,
                PlatformVersion = _detectedVersion,
            };
        }
    }
}
