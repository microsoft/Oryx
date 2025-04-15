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
            var (versionProvider, onDiskVersionProvider, storageVersionProvider, externalVersionProvider) = CreateVersionProvider(
                enableDynamicInstall: true);

            // Act
            var versionInfo = versionProvider.GetVersionInfo();

            // Assert
            Assert.True(storageVersionProvider.GetVersionInfoCalled);
            Assert.False(externalVersionProvider.GetVersionInfoCalled);
            Assert.False(onDiskVersionProvider.GetVersionInfoCalled);
        }

        [Fact]
        public void GetsVersions_DoesNotGetVersionsFromStorage_WhenDynamicInstall_IsFalse()
        {
            // Arrange
            var (versionProvider, onDiskVersionProvider, storageVersionProvider, externalVersionProvider) = CreateVersionProvider(
                enableDynamicInstall: false);

            // Act
            var versionInfo = versionProvider.GetVersionInfo();

            // Assert
            Assert.False(storageVersionProvider.GetVersionInfoCalled);
            Assert.False(externalVersionProvider.GetVersionInfoCalled);
            Assert.True(onDiskVersionProvider.GetVersionInfoCalled);
        }

        [Fact]
        public void GetsVersions_DoesNotGetVersionsFromStorage_ByDefault()
        {
            // Arrange
            var (versionProvider, onDiskVersionProvider, storageVersionProvider, externalVersionProvider) = CreateVersionProvider(
                enableDynamicInstall: false);

            // Act
            var versionInfo = versionProvider.GetVersionInfo();

            // Assert
            Assert.False(storageVersionProvider.GetVersionInfoCalled);
            Assert.False(externalVersionProvider.GetVersionInfoCalled);
            Assert.True(onDiskVersionProvider.GetVersionInfoCalled);
        }

        [Fact]
        public void GetsVersions_UsesExternalVersionProvider_WhenExternalProviderAndDynamicInstallEnabled()
        {
            // Arrange
            var (versionProvider, onDiskVersionProvider, storageVersionProvider, externalVersionProvider) = CreateVersionProvider(
                enableDynamicInstall: true, enableExternalSdkProvider: true);

            // Act
            var versionInfo = versionProvider.GetVersionInfo();

            // Assert
            Assert.True(externalVersionProvider.GetVersionInfoCalled);
            Assert.False(storageVersionProvider.GetVersionInfoCalled);
            Assert.False(onDiskVersionProvider.GetVersionInfoCalled);
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

        private class TestPhpExternalVersionProvider : PhpExternalVersionProvider
        {
            public TestPhpExternalVersionProvider(
                IOptions<BuildScriptGeneratorOptions> commonOptions, IExternalSdkProvider externalProvider, ILoggerFactory loggerFactory)
                : base(commonOptions, externalProvider, loggerFactory)
            {
            }

            public bool GetVersionInfoCalled { get; private set; }

            public override PlatformVersionInfo GetVersionInfo()
            {
                GetVersionInfoCalled = true;

                return null;
            }
        }

        private (IPhpVersionProvider, TestPhpOnDiskVersionProvider, TestPhpSdkStorageVersionProvider, TestPhpExternalVersionProvider)
            CreateVersionProvider(bool enableDynamicInstall, bool enableExternalSdkProvider = false)
        {
            var commonOptions = Options.Create(new BuildScriptGeneratorOptions()
            {
                EnableDynamicInstall = enableDynamicInstall,
                EnableExternalSdkProvider = enableExternalSdkProvider,
            });

            var onDiskProvider = new TestPhpOnDiskVersionProvider();
            var storageProvider = new TestPhpSdkStorageVersionProvider(
                commonOptions,
                new TestHttpClientFactory(),
                NullLoggerFactory.Instance);
            var externalProvider = new TestPhpExternalVersionProvider(
                commonOptions,
                new TestExternalSdkProvider(),
                NullLoggerFactory.Instance);
            var versionProvider = new PhpVersionProvider(
                commonOptions,
                onDiskProvider,
                storageProvider,
                externalProvider,
                NullLogger<PhpVersionProvider>.Instance);
            return (versionProvider, onDiskProvider, storageProvider, externalProvider);
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
