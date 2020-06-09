// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Detector.DotNetCore;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotNetCore
{
    public class DotNetCoreScriptGeneratorServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddDotNetCoreScriptGeneratorServices_AddsProjectFileProvidersInExpectedOrder()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddDotNetCoreScriptGeneratorServices();

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
    }
}
