// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    /// <summary>
    /// Tests for <see cref="ExternalAcrVersionProviderBase"/> exercised through
    /// <see cref="DotNetCoreExternalAcrVersionProvider"/>, the thinnest concrete subclass.
    /// Validates the DebianFlavor guard and the never-throws contract.
    /// </summary>
    public class ExternalAcrVersionProviderBaseTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetSdkVersion_ReturnsNull_WhenDebianFlavorIsNullOrEmpty(string debianFlavor)
        {
            var provider = CreateProvider(debianFlavor: debianFlavor);

            var version = provider.GetSdkVersion();

            Assert.Null(version);
        }

        [Fact]
        public void GetSdkVersion_NeverThrows_WhenSocketIsUnavailable()
        {
            // GetCompanionSdkVersion has try/catch around SendRequestAsync.
            // No exception should ever propagate to the caller.
            // DotNetCorePlatform.ResolveVersions() also has a try/catch around
            // GetSdkVersion() for defense-in-depth — but GetSdkVersion itself
            // must never throw, matching the contract of other version providers.
            var provider = CreateProvider(debianFlavor: "bookworm");

            var ex = Record.Exception(() => provider.GetSdkVersion());

            Assert.Null(ex);
        }

        private static DotNetCoreExternalAcrVersionProvider CreateProvider(string debianFlavor)
        {
            var options = Options.Create(new BuildScriptGeneratorOptions
            {
                DebianFlavor = debianFlavor,
            });

            return new DotNetCoreExternalAcrVersionProvider(
                options,
                NullLoggerFactory.Instance,
                new DefaultStandardOutputWriter());
        }
    }
}
