// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Hugo;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Hugo
{
    internal class TestHugoPlatformDetector : IHugoPlatformDetector
    {
        private readonly string _detectedVersion;

        public TestHugoPlatformDetector(string detectedVersion = null)
        {
            _detectedVersion = detectedVersion;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            return new PlatformDetectorResult
            {
                Platform = Detector.Hugo.HugoConstants.PlatformName,
                PlatformVersion = _detectedVersion,
            };
        }
    }
}
