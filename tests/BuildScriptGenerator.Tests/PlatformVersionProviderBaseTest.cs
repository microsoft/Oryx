// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    /// <summary>
    /// Tests for <see cref="PlatformVersionProviderBase"/> edge cases NOT covered by
    /// NodeVersionProviderTest (which tests the full 4-tier chain through the real
    /// NodeVersionProvider). These tests target specific boundary behaviors.
    /// </summary>
    public class PlatformVersionProviderBaseTest
    {
        [Fact]
        public void GetVersionInfo_FallsToNext_WhenProviderReturnsEmptySupportedVersions()
        {
            // HasSupportedVersions checks SupportedVersions.Any() — empty list should fall through.
            var emptyVersionInfo = PlatformVersionInfo.CreateAvailableOnAcr(
                supportedVersions: Array.Empty<string>(),
                defaultVersion: null);
            var expected = CreateVersionInfo("6.0.0");

            var provider = CreateProvider(
                enableDynamicInstall: true,
                enableExternalAcr: true,
                externalAcrResult: emptyVersionInfo,
                cdnResult: expected);

            var result = provider.GetVersionInfo();

            Assert.Same(expected, result);
            Assert.True(provider.ExternalAcrCalled);
            Assert.True(provider.CdnCalled);
        }

        [Fact]
        public void GetVersionInfo_CachesResult_ProvidersNotReInvoked()
        {
            // PlatformVersionProviderBase caches versionInfo per instance.
            var expected = CreateVersionInfo("9.0.0");
            var provider = CreateProvider(
                enableDynamicInstall: true,
                enableExternalAcr: true,
                externalAcrResult: expected);

            var result1 = provider.GetVersionInfo();
            provider.ResetCallFlags();
            var result2 = provider.GetVersionInfo();

            Assert.Same(result1, result2);
            Assert.False(provider.ExternalAcrCalled);
            Assert.False(provider.CdnCalled);
        }

        private static PlatformVersionInfo CreateVersionInfo(string version)
        {
            return PlatformVersionInfo.CreateAvailableOnAcr(
                supportedVersions: new[] { version },
                defaultVersion: version);
        }

        private static TestableVersionProvider CreateProvider(
            bool enableDynamicInstall = false,
            bool enableExternalAcr = false,
            bool enableExternalSdk = false,
            bool enableDirectAcr = false,
            PlatformVersionInfo externalAcrResult = null,
            PlatformVersionInfo externalBlobResult = null,
            PlatformVersionInfo directAcrResult = null,
            PlatformVersionInfo cdnResult = null,
            PlatformVersionInfo onDiskResult = null,
            bool externalAcrThrows = false,
            bool externalBlobThrows = false,
            bool directAcrThrows = false)
        {
            var options = new BuildScriptGeneratorOptions
            {
                EnableDynamicInstall = enableDynamicInstall,
                EnableExternalAcrSdkProvider = enableExternalAcr,
                EnableExternalSdkProvider = enableExternalSdk,
                EnableAcrSdkProvider = enableDirectAcr,
            };

            return new TestableVersionProvider(
                options,
                NullLoggerFactory.Instance.CreateLogger("test"),
                new DefaultStandardOutputWriter(),
                externalAcrResult: externalAcrResult,
                externalBlobResult: externalBlobResult,
                directAcrResult: directAcrResult,
                cdnResult: cdnResult,
                onDiskResult: onDiskResult,
                externalAcrThrows: externalAcrThrows,
                externalBlobThrows: externalBlobThrows,
                directAcrThrows: directAcrThrows);
        }

        /// <summary>
        /// Concrete subclass that exposes provider calls as controllable stubs.
        /// </summary>
        private class TestableVersionProvider : PlatformVersionProviderBase
        {
            private readonly PlatformVersionInfo externalAcrResult;
            private readonly PlatformVersionInfo externalBlobResult;
            private readonly PlatformVersionInfo directAcrResult;
            private readonly PlatformVersionInfo cdnResult;
            private readonly PlatformVersionInfo onDiskResult;
            private readonly bool externalAcrThrows;
            private readonly bool externalBlobThrows;
            private readonly bool directAcrThrows;

            public TestableVersionProvider(
                BuildScriptGeneratorOptions options,
                ILogger logger,
                IStandardOutputWriter outputWriter,
                PlatformVersionInfo externalAcrResult = null,
                PlatformVersionInfo externalBlobResult = null,
                PlatformVersionInfo directAcrResult = null,
                PlatformVersionInfo cdnResult = null,
                PlatformVersionInfo onDiskResult = null,
                bool externalAcrThrows = false,
                bool externalBlobThrows = false,
                bool directAcrThrows = false)
                : base(options, logger, outputWriter)
            {
                this.externalAcrResult = externalAcrResult;
                this.externalBlobResult = externalBlobResult;
                this.directAcrResult = directAcrResult;
                this.cdnResult = cdnResult;
                this.onDiskResult = onDiskResult;
                this.externalAcrThrows = externalAcrThrows;
                this.externalBlobThrows = externalBlobThrows;
                this.directAcrThrows = directAcrThrows;
            }

            public bool ExternalAcrCalled { get; private set; }

            public bool ExternalBlobCalled { get; private set; }

            public bool DirectAcrCalled { get; private set; }

            public bool CdnCalled { get; private set; }

            public bool OnDiskCalled { get; private set; }

            public void ResetCallFlags()
            {
                this.ExternalAcrCalled = false;
                this.ExternalBlobCalled = false;
                this.DirectAcrCalled = false;
                this.CdnCalled = false;
                this.OnDiskCalled = false;
            }

            protected override string PlatformName => "test-platform";

            protected override PlatformVersionInfo GetOnDiskVersionInfo()
            {
                this.OnDiskCalled = true;
                return this.onDiskResult;
            }

            protected override PlatformVersionInfo GetSdkStorageVersionInfo()
            {
                this.CdnCalled = true;
                return this.cdnResult;
            }

            protected override PlatformVersionInfo GetExternalVersionInfo()
            {
                this.ExternalBlobCalled = true;
                if (this.externalBlobThrows)
                {
                    throw new Exception("External blob simulated failure");
                }

                return this.externalBlobResult;
            }

            protected override PlatformVersionInfo GetExternalAcrVersionInfo()
            {
                this.ExternalAcrCalled = true;
                if (this.externalAcrThrows)
                {
                    throw new Exception("External ACR simulated failure");
                }

                return this.externalAcrResult;
            }

            protected override PlatformVersionInfo GetAcrVersionInfo()
            {
                this.DirectAcrCalled = true;
                if (this.directAcrThrows)
                {
                    throw new Exception("Direct ACR simulated failure");
                }

                return this.directAcrResult;
            }
        }
    }
}
