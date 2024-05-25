// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Hugo;
using Microsoft.Oryx.Detector.Hugo;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Hugo
{
    public class HugoPlatformTest
    {
        [Fact]
        public void Detect_ReturnsDefaultVersion_IfNoVersionFoundInOptions()
        {
            // Arrange
            var expectedVersion = BuildScriptGenerator.Hugo.HugoConstants.Version;
            var detector = CreateDetector(detectedVersion: null);
            var platform = CreatePlatform(detector);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BuildScriptGenerator.Hugo.HugoConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsVersionFromOptions()
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var detectedVersion = "3.4.5";
            var hugoScriptGeneratorOptions = new HugoScriptGeneratorOptions
            {
                HugoVersion = expectedVersion
            };
            var detector = CreateDetector(detectedVersion: detectedVersion);
            var platform = CreatePlatform(
                detector,
                hugoScriptGeneratorOptions: hugoScriptGeneratorOptions);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BuildScriptGenerator.Hugo.HugoConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        private IHugoPlatformDetector CreateDetector(string detectedVersion)
        {
            return new TestHugoPlatformDetector(detectedVersion: detectedVersion);
        }

        private BuildScriptGeneratorContext CreateContext()
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = new MemorySourceRepo(),
            };
        }

        private HugoPlatform CreatePlatform(
            IHugoPlatformDetector detector,
            BuildScriptGeneratorOptions buildScriptGeneratorOptions = null,
            HugoScriptGeneratorOptions hugoScriptGeneratorOptions = null)
        {
            buildScriptGeneratorOptions = buildScriptGeneratorOptions ?? new BuildScriptGeneratorOptions();
            hugoScriptGeneratorOptions = hugoScriptGeneratorOptions ?? new HugoScriptGeneratorOptions();
            return new HugoPlatform(
                Options.Create(buildScriptGeneratorOptions),
                Options.Create(hugoScriptGeneratorOptions),
                NullLogger<HugoPlatform>.Instance,
                new HugoPlatformInstaller(Options.Create(buildScriptGeneratorOptions), NullLoggerFactory.Instance),
                detector, 
                TelemetryClientHelper.GetTelemetryClient());
        }
    }
}
