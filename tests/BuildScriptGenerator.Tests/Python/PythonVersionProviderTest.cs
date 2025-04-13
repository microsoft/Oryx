﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Python
{
    public class PythonVersionProviderTest
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

        private class TestPythonExternalVersionProvider : PythonExternalVersionProvider
        {
            public TestPythonExternalVersionProvider(
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

        private class TestPythonSdkStorageVersionProvider : PythonSdkStorageVersionProvider
        {
            public TestPythonSdkStorageVersionProvider(
                IOptions<BuildScriptGeneratorOptions> commonOptions, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
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

        private (IPythonVersionProvider, TestPythonOnDiskVersionProvider, TestPythonSdkStorageVersionProvider, TestPythonExternalVersionProvider)
            CreateVersionProvider(bool enableDynamicInstall, bool enableExternalSdkProvider = false)
        {
            var commonOptions = Options.Create(new BuildScriptGeneratorOptions()
            {
                EnableDynamicInstall = enableDynamicInstall,
                EnableExternalSdkProvider = enableExternalSdkProvider,
            });

            var onDiskProvider = new TestPythonOnDiskVersionProvider();
            var storageProvider = new TestPythonSdkStorageVersionProvider(
                commonOptions,
                new TestHttpClientFactory(),
                NullLoggerFactory.Instance);
            var externalProvider = new TestPythonExternalVersionProvider(
                commonOptions,
                new TestExternalSdkProvider(),
                NullLoggerFactory.Instance);
            var versionProvider = new PythonVersionProvider(
                commonOptions,
                onDiskProvider,
                storageProvider,
                externalProvider,
                NullLogger<PythonVersionProvider>.Instance);
            return (versionProvider, onDiskProvider, storageProvider, externalProvider);
        }

        private class TestPythonOnDiskVersionProvider : PythonOnDiskVersionProvider
        {
            public TestPythonOnDiskVersionProvider()
                : base(NullLogger<PythonOnDiskVersionProvider>.Instance)
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
