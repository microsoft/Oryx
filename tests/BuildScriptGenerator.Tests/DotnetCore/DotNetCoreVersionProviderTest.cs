// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DotNetCoreVersionProviderTest
    {
        [Fact]
        public void SupportedDotNetCoreVersions_ReturnsVersionsAvailableFromStorage_WhenUseLatestVersion_IsTrue()
        {
            // Arrange
            var expectedVersions = new[] { "1.0.0", "2.2.0" };
            var environment = new TestEnvironment();
            environment.Variables[SdkStorageConstants.UseLatestVersion] = "true";
            var platformInstaller = GetPlatformInstaller(environment, expectedVersions);
            var versionProvider = new DotNetCoreVersionProvider(
                Options.Create(new DotNetCoreScriptGeneratorOptions()),
                environment,
                platformInstaller);

            // Act
            var actualVersions = versionProvider.SupportedDotNetCoreVersions;

            // Assert
            Assert.Same(expectedVersions, actualVersions);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(false)]
        public void SupportedDotNetCoreVersions_ReturnsVersionsOnDisk_WhenUseLatestVersion_IsFalseOrNotAvailable(
            bool? useLatestVersion)
        {
            // Arrange
            var options = new DotNetCoreScriptGeneratorOptions();
            options.SupportedVersions = new List<string>();
            options.SupportedVersions.Add("1.0.0");
            options.SupportedVersions.Add("2.2.0");
            var versionOnWeb = new[] { "1.0.1", "2.2.1" };
            var environment = new TestEnvironment();
            if (useLatestVersion.HasValue)
            {
                environment.Variables[SdkStorageConstants.UseLatestVersion] = useLatestVersion.Value.ToString();
            }
            var platformInstaller = GetPlatformInstaller(environment, versionOnWeb);
            var versionProvider = new DotNetCoreVersionProvider(
                Options.Create(options),
                environment,
                platformInstaller);

            // Act
            var actualVersions = versionProvider.SupportedDotNetCoreVersions;

            // Assert
            Assert.NotNull(actualVersions);
            Assert.Equal(2, actualVersions.Count());
            Assert.Equal("1.0.0", actualVersions.ElementAt(0));
            Assert.Equal("2.2.0", actualVersions.ElementAt(1));
        }

        private DotNetCorePlatformInstaller GetPlatformInstaller(
            IEnvironment environment,
            IEnumerable<string> availableVersionsOnWeb)
        {
            var options = Options.Create(new BuildScriptGeneratorOptions());
            var clientFactory = new TestHttpClientFactory();
            return new TestPlatformInstaller(options, environment, clientFactory, availableVersionsOnWeb);
        }

        private class TestPlatformInstaller : DotNetCorePlatformInstaller
        {
            private readonly IEnumerable<string> _availableVersionsOnWeb;

            public TestPlatformInstaller(
                IOptions<BuildScriptGeneratorOptions> commonOptions,
                IEnvironment environment,
                IHttpClientFactory httpClientFactory,
                IEnumerable<string> availableVersionsOnWeb)
                : base(commonOptions, environment, httpClientFactory)
            {
                _availableVersionsOnWeb = availableVersionsOnWeb;
            }

            public override IEnumerable<string> GetAvailableVersionsInStorage()
            {
                return _availableVersionsOnWeb;
            }
        }
    }
}
