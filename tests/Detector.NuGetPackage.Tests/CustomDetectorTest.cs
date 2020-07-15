// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Detector.NuGetPackage.Tests
{
    public class CustomDetectorTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempRootDirPath;

        public CustomDetectorTest(TestTempDirTestFixture testTempDirTestFixture)
        {
            _tempRootDirPath = testTempDirTestFixture.RootDirPath;
        }

        [Fact]
        public void GetAllDetectedPlatforms_InvokesCustomDetector()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();
            services
                .AddPlatformDetectorServices()
                .AddSingleton<IConfiguration>(configuration)
                .TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformDetector, TestDetector>());

            var serviceProvider = services.BuildServiceProvider();
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempRootDirPath, Guid.NewGuid().ToString()))
                .FullName;
            var context = new DetectorContext
            {
                SourceRepo = new LocalSourceRepo(sourceDir)
            };
            var defaultPlatformDetector = serviceProvider.GetRequiredService<IDetector>();

            // Act
            var detectorResults = defaultPlatformDetector.GetAllDetectedPlatforms(context);

            // Assert
            Assert.NotNull(detectorResults);
            var result = Assert.Single(detectorResults);
            Assert.Equal("test", result.Platform);
            Assert.Equal("1.0.0", result.PlatformVersion);
        }

        private class TestDetector : IPlatformDetector
        {
            public PlatformDetectorResult Detect(DetectorContext context)
            {
                return new PlatformDetectorResult
                {
                    Platform = "test",
                    PlatformVersion = "1.0.0",
                };
            }
        }
    }
}
