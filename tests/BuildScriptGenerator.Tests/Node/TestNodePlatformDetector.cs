// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Node;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    internal class TestNodePlatformDetector : INodePlatformDetector
    {
        private readonly string _detectedVersion;

        public TestNodePlatformDetector(string detectedVersion = null)
        {
            _detectedVersion = detectedVersion;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            return new PlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = _detectedVersion,
            };
        }
    }
}
