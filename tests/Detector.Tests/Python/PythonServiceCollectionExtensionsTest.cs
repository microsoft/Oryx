// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.Detector.Python;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests.Python
{
    public class PythonServiceCollectionExtensionsTest
    {
        [Fact]
        public void ReturnsSameInstanceOfDetectorWhenResolvedAsIPythonPlatformDetector()
        {
            // Arrange
            var services = new ServiceCollection();
            services
                .AddLogging()
                .AddPythonServices();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var instance1 = serviceProvider.GetRequiredService<IPythonPlatformDetector>();
            var instance2 = serviceProvider.GetRequiredService<IPythonPlatformDetector>();

            // Assert
            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void ReturnsSameInstanceOfDetectorWhenResolvedFromDifferentInterfaceTypes()
        {
            // Arrange
            var services = new ServiceCollection();
            services
                .AddLogging()
                .AddPythonServices();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var detectors = serviceProvider.GetRequiredService<IEnumerable<IPlatformDetector>>();
            var instance2 = serviceProvider.GetRequiredService<IPythonPlatformDetector>();

            // Assert
            Assert.NotNull(detectors);
            var instance1 = Assert.Single(detectors);
            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void ReturnsSameInstanceOfDetectorWhenResolvedAsIPlatformDetector()
        {
            // Arrange
            var services = new ServiceCollection();
            services
                .AddLogging()
                .AddPythonServices();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var detectors1 = serviceProvider.GetRequiredService<IEnumerable<IPlatformDetector>>();
            var detectors2 = serviceProvider.GetRequiredService<IEnumerable<IPlatformDetector>>();

            // Assert
            Assert.NotNull(detectors1);
            var instance1 = Assert.Single(detectors1);
            Assert.NotNull(detectors2);
            var instance2 = Assert.Single(detectors2);
            Assert.Same(instance1, instance2);
        }
    }
}
