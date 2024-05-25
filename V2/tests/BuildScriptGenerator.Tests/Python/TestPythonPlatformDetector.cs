// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Python;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Python
{
    internal class TestPythonPlatformDetector : IPythonPlatformDetector
    {
        private readonly string _detectedVersion;

        public TestPythonPlatformDetector(string detectedVersion = null)
        {
            _detectedVersion = detectedVersion;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            return new PlatformDetectorResult
            {
                Platform = PythonConstants.PlatformName,
                PlatformVersion = _detectedVersion,
            };
        }
    }
}
