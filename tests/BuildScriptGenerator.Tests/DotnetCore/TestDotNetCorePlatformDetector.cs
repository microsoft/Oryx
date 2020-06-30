// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.DotNetCore;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotNetCore
{
    internal class TestDotNetCorePlatformDetector : IDotNetCorePlatformDetector
    {
        private readonly string _detectedVersion;
        private readonly string _detectedProjectFile;

        public TestDotNetCorePlatformDetector(string detectedVersion = null, string detectedProjectFile = null)
        {
            _detectedVersion = detectedVersion;
            _detectedProjectFile = detectedProjectFile;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            return new DotNetCorePlatformDetectorResult
            {
                Platform = DotNetCoreConstants.PlatformName,
                PlatformVersion = _detectedVersion,
                ProjectFile = _detectedProjectFile,
            };
        }
    }
}
