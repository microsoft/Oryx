// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Golang;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Golang;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Golang
{
    internal class TestGolangPlatformDetector : IGolangPlatformDetector
    {
        private readonly string _detectedVersion;

        public TestGolangPlatformDetector(string detectedVersion = null)
        {
            _detectedVersion = detectedVersion;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            return new GolangPlatformDetectorResult
            {
                Platform = GolangConstants.PlatformName,
                PlatformVersion = _detectedVersion,
            };
        }
    }
}