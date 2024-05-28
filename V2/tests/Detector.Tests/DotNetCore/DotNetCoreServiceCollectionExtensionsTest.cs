// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.Detector.DotNetCore;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests.DotNetCore
{
    public class DotNetCoreServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddDotNetCoreScriptGeneratorServices_AddsProjectFileProvidersInExpectedOrder()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddDotNetCoreServices();

            // Assert
            var projectFileProviders = services.Where(sd => sd.ServiceType == typeof(IProjectFileProvider));
            Assert.NotNull(projectFileProviders);
            Assert.Equal(3, projectFileProviders.Count());
            Assert.Equal(
                typeof(ExplicitProjectFileProvider),
                projectFileProviders.ElementAt(0).ImplementationType);
            Assert.Equal(
                typeof(RootDirectoryProjectFileProvider),
                projectFileProviders.ElementAt(1).ImplementationType);
            Assert.Equal(
                typeof(ProbeAndFindProjectFileProvider),
                projectFileProviders.ElementAt(2).ImplementationType);
        }

        [Fact]
        public void ReturnsSameInstanceOfDetectorWhenResolvedAsIDotNetCorePlatformDetector()
        {
            // Arrange
            var services = new ServiceCollection();
            services
                .AddLogging()
                .AddDotNetCoreServices();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var instance1 = serviceProvider.GetRequiredService<IDotNetCorePlatformDetector>();
            var instance2 = serviceProvider.GetRequiredService<IDotNetCorePlatformDetector>();

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
                .AddDotNetCoreServices();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var detectors = serviceProvider.GetRequiredService<IEnumerable<IPlatformDetector>>();
            var instance2 = serviceProvider.GetRequiredService<IDotNetCorePlatformDetector>();

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
                .AddDotNetCoreServices();
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
