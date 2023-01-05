// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Detector.DotNetCore;
using Microsoft.Oryx.Detector.Node;
using Moq;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests
{
    public class DefaultPlatformDetectorTest
    {
        [Fact]
        public void Detect_ReturnsResult_MultiplePlatform_DotNetCoreReactApp()
        {
            Mock<IPlatformDetector> mockNodePlatformDetector = new Mock<IPlatformDetector>();
            Mock<IPlatformDetector> mockDotnetcorePlatformDetector = new Mock<IPlatformDetector>();
            IEnumerable<IPlatformDetector> platformDetectors = new List<IPlatformDetector>() { mockNodePlatformDetector.Object, mockDotnetcorePlatformDetector.Object };
            
            var options = new Mock<IOptions<DetectorOptions>>();
            var sourceRepo = new MemorySourceRepo();
            var detector = new DefaultPlatformDetector(
                platformDetectors,
                NullLogger<DefaultPlatformDetector>.Instance);
            var context = CreateContext(sourceRepo);

            var detectionResult1 = new PlatformDetectorResult();
            detectionResult1.Platform = NodeConstants.PlatformName;
            detectionResult1.PlatformVersion = "12.16.1";

            var detectionResult2 = new PlatformDetectorResult();
            detectionResult2.Platform = DotNetCoreConstants.PlatformName;
            detectionResult2.PlatformVersion = "3.1";

            mockNodePlatformDetector.Setup(x => x.Detect(context)).Returns(detectionResult1);
            mockDotnetcorePlatformDetector.Setup(x => x.Detect(context)).Returns(detectionResult2);

            IPlatformDetector nodePlatformDetector = mockNodePlatformDetector.Object;
            IPlatformDetector dotnetcorePlatformDetector = mockDotnetcorePlatformDetector.Object;

            // Act
            var detectionResults = detector.GetAllDetectedPlatforms(context);

            // Assert
            Assert.NotNull(detectionResults);
            Assert.Equal(2, detectionResults.Count());
        }

        private DetectorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new DetectorContext
            {
                SourceRepo = sourceRepo,
            };
        }
    }
}
