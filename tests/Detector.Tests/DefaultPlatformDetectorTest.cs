// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Moq;
using Microsoft.Oryx.Detector;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector.Tests
{
    public class DefaultPlatformDetectorTest
    {
        [Fact]
        public void Detect_ReturnsResult_MultiplePlatform_DotNetCoreReactApp()
        {
            var platformDetectorProvider = new Mock<IPlatformDetectorProvider>();
            
            var options = new Mock<IOptions<DetectorOptions>>();
            var sourceRepo = new MemorySourceRepo();
            var detector = new DefaultPlatformDetector(
                platformDetectorProvider.Object, 
                NullLogger<DefaultPlatformDetector>.Instance,
                options.Object);
            var context = CreateContext(sourceRepo);

            var detectionResult1 = new PlatformDetectorResult();
            detectionResult1.Platform = "node";
            detectionResult1.PlatformVersion = "12.16.1";

            var detectionResult2 = new PlatformDetectorResult();
            detectionResult2.Platform = "dotnetcore";
            detectionResult2.PlatformVersion = "3.1";

            Mock<IPlatformDetector> mockNodePlatformDetector = new Mock<IPlatformDetector>();
            Mock<IPlatformDetector> mockDotnetcorePlatformDetector = new Mock<IPlatformDetector>();
            
            mockNodePlatformDetector.Setup(x => x.Detect(context)).Returns(detectionResult1);
            mockDotnetcorePlatformDetector.Setup(x => x.Detect(context)).Returns(detectionResult2);

            IPlatformDetector nodePlatformDetector = mockNodePlatformDetector.Object;
            IPlatformDetector dotnetcorePlatformDetector = mockDotnetcorePlatformDetector.Object;
            
            platformDetectorProvider.Setup(x => x.TryGetDetector(PlatformName.Node, out nodePlatformDetector)).Returns(true);
            platformDetectorProvider.Setup(x => x.TryGetDetector(PlatformName.DotNetCore, out dotnetcorePlatformDetector)).Returns(true);
            
            // Act
            var detectionResults = detector.GetAllDetectedPlatforms(context);

            // Assert
            Assert.NotNull(detectionResults);
            Assert.Equal(2, detectionResults.Count);
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
