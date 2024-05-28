// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.Detector.Ruby;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests.Ruby
{
    public class RubyServiceCollectionExtensionsTest
    {
        [Fact]
        public void ReturnsSameInstanceOfDetectorWhenResolvedAsIRubyPlatformDetector()
        {
            // Arrange
            var services = new ServiceCollection();
            services
                .AddLogging()
                .AddRubyServices();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var instance1 = serviceProvider.GetRequiredService<IRubyPlatformDetector>();
            var instance2 = serviceProvider.GetRequiredService<IRubyPlatformDetector>();

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
                .AddRubyServices();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var detectors = serviceProvider.GetRequiredService<IEnumerable<IPlatformDetector>>();
            var instance2 = serviceProvider.GetRequiredService<IRubyPlatformDetector>();

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
                .AddRubyServices();
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
