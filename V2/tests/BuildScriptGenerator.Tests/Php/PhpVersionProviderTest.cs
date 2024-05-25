// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Php
{
    public class PhpVersionProviderTest
    {
        [Fact]
        public void GetsVersions_FromStorage_WhenDynamicInstall_IsEnabled()
        {
            // Arrange
            var (versionProvider, onDiskVersionProvider, storageVersionProvider) = CreateVersionProvider(
                enableDynamicInstall: true);

            // Act
            var versionInfo = versionProvider.GetVersionInfo();

            // Assert
            Assert.True(storageVersionProvider.GetVersionInfoCalled);
            Assert.False(onDiskVersionProvider.GetVersionInfoCalled);
        }

        [Fact]
        public void GetsVersions_DoesNotGetVersionsFromStorage_WhenDynamicInstall_IsFalse()
        {
            // Arrange
            var (versionProvider, onDiskVersionProvider, storageVersionProvider) = CreateVersionProvider(
                enableDynamicInstall: false);

            // Act
            var versionInfo = versionProvider.GetVersionInfo();

            // Assert
            Assert.False(storageVersionProvider.GetVersionInfoCalled);
            Assert.True(onDiskVersionProvider.GetVersionInfoCalled);
        }

        [Fact]
        public void GetsVersions_DoesNotGetVersionsFromStorage_ByDefault()
        {
            // Arrange
            var (versionProvider, onDiskVersionProvider, storageVersionProvider) = CreateVersionProvider(
                enableDynamicInstall: false);

            // Act
            var versionInfo = versionProvider.GetVersionInfo();

            // Assert
            Assert.False(storageVersionProvider.GetVersionInfoCalled);
            Assert.True(onDiskVersionProvider.GetVersionInfoCalled);
        }

        private class TestPhpSdkStorageVersionProvider : PhpSdkStorageVersionProvider
        {
            public TestPhpSdkStorageVersionProvider(
                IOptions<BuildScriptGeneratorOptions> commonOptions,
                IHttpClientFactory httpClientFactory,
                ILoggerFactory loggerFactory)
                : base(commonOptions, httpClientFactory, loggerFactory)
            {
            }

            public bool GetVersionInfoCalled { get; private set; }

            public override PlatformVersionInfo GetVersionInfo()
            {
                GetVersionInfoCalled = true;

                return null;
            }
        }

        private (IPhpVersionProvider, TestPhpOnDiskVersionProvider, TestPhpSdkStorageVersionProvider)
            CreateVersionProvider(bool enableDynamicInstall)
        {
            var commonOptions = Options.Create(new BuildScriptGeneratorOptions()
            {
                EnableDynamicInstall = enableDynamicInstall
            });

            var onDiskProvider = new TestPhpOnDiskVersionProvider();
            var storageProvider = new TestPhpSdkStorageVersionProvider(
                commonOptions,
                new TestHttpClientFactory(),
                NullLoggerFactory.Instance);
            var versionProvider = new PhpVersionProvider(
                commonOptions,
                onDiskProvider,
                storageProvider,
                NullLogger<PhpVersionProvider>.Instance);
            return (versionProvider, onDiskProvider, storageProvider);
        }

        private class TestPhpOnDiskVersionProvider : PhpOnDiskVersionProvider
        {
            public TestPhpOnDiskVersionProvider()
                : base(NullLogger<PhpOnDiskVersionProvider>.Instance)
            {
            }

            public bool GetVersionInfoCalled { get; private set; }

            public override PlatformVersionInfo GetVersionInfo()
            {
                GetVersionInfoCalled = true;

                return null;
            }
        }
    }
}
