// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Hugo;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Hugo
{
    public class HugoPlatformTest
    {
        [Fact]
        public void Detect_ReturnsDefaultVersion_IfNoVersionFoundInOptions()
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var platform = CreatePlatform(detectedVersion: expectedVersion);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HugoConstants.PlatformName, result.Platform);
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
            var platform = CreatePlatform(
                detectedVersion: detectedVersion,
                hugoScriptGeneratorOptions: hugoScriptGeneratorOptions);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HugoConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        private BuildScriptGeneratorContext CreateContext()
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = new MemorySourceRepo(),
            };
        }

        private HugoPlatform CreatePlatform(
            string detectedVersion = null,
            BuildScriptGeneratorOptions buildScriptGeneratorOptions = null,
            HugoScriptGeneratorOptions hugoScriptGeneratorOptions = null)
        {
            detectedVersion = detectedVersion ?? HugoConstants.Version;
            buildScriptGeneratorOptions = buildScriptGeneratorOptions ?? new BuildScriptGeneratorOptions();
            hugoScriptGeneratorOptions = hugoScriptGeneratorOptions ?? new HugoScriptGeneratorOptions();
            return new HugoPlatform(
                Options.Create(buildScriptGeneratorOptions),
                Options.Create(hugoScriptGeneratorOptions),
                NullLogger<HugoPlatform>.Instance,
                new HugoPlatformInstaller(Options.Create(buildScriptGeneratorOptions), NullLoggerFactory.Instance),
                new TestHugoPlatformDetector(detectedVersion));
        }

        private class TestHugoPlatformDetector : HugoPlatformDetector
        {
            private readonly string _detectedVersion;

            public TestHugoPlatformDetector(string detectedVersion)
                : base(new TestEnvironment())
            {
                _detectedVersion = detectedVersion;
            }

            public override PlatformDetectorResult Detect(RepositoryContext context)
            {
                return new PlatformDetectorResult
                {
                    Platform = HugoConstants.PlatformName,
                    PlatformVersion = _detectedVersion
                };
            }
        }
    }
}
