// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
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

        [Fact]
        public void GetsVersions_UsesExternalAcrProvider_WhenExternalAcrProviderAndDynamicInstallEnabled()
        {
            // Arrange
            var result = CreateVersionProviderWithAcr(
                enableDynamicInstall: true,
                enableExternalAcrSdkProvider: true);

            // Act
            var versionInfo = result.VersionProvider.GetVersionInfo();

            // Assert
            Assert.True(result.ExternalAcrVersionProvider.GetVersionInfoCalled);
            Assert.False(result.AcrVersionProvider.GetVersionInfoCalled);
            Assert.False(result.StorageVersionProvider.GetVersionInfoCalled);
            Assert.False(result.OnDiskVersionProvider.GetVersionInfoCalled);
        }

        [Fact]
        public void GetsVersions_UsesAcrProvider_WhenAcrProviderAndDynamicInstallEnabled()
        {
            // Arrange
            var result = CreateVersionProviderWithAcr(
                enableDynamicInstall: true,
                enableAcrSdkProvider: true);

            // Act
            var versionInfo = result.VersionProvider.GetVersionInfo();

            // Assert
            Assert.True(result.AcrVersionProvider.GetVersionInfoCalled);
            Assert.False(result.ExternalAcrVersionProvider.GetVersionInfoCalled);
            Assert.False(result.StorageVersionProvider.GetVersionInfoCalled);
            Assert.False(result.OnDiskVersionProvider.GetVersionInfoCalled);
        }

        [Fact]
        public void GetsVersions_ExternalAcrTakesPriority_OverExternalSdk()
        {
            // Arrange
            var result = CreateVersionProviderWithAcr(
                enableDynamicInstall: true,
                enableExternalAcrSdkProvider: true,
                enableExternalSdkProvider: true);

            // Act
            var versionInfo = result.VersionProvider.GetVersionInfo();

            // Assert
            Assert.True(result.ExternalAcrVersionProvider.GetVersionInfoCalled);
            Assert.False(result.ExternalVersionProvider.GetVersionInfoCalled);
            Assert.False(result.StorageVersionProvider.GetVersionInfoCalled);
        }

        [Fact]
        public void GetsVersions_ExternalAcrTakesPriority_OverDirectAcr()
        {
            // Arrange
            var result = CreateVersionProviderWithAcr(
                enableDynamicInstall: true,
                enableExternalAcrSdkProvider: true,
                enableAcrSdkProvider: true);

            // Act
            var versionInfo = result.VersionProvider.GetVersionInfo();

            // Assert
            Assert.True(result.ExternalAcrVersionProvider.GetVersionInfoCalled);
            Assert.False(result.AcrVersionProvider.GetVersionInfoCalled);
            Assert.False(result.StorageVersionProvider.GetVersionInfoCalled);
        }

        [Fact]
        public void GetsVersions_FallsBackToStorage_WhenExternalAcrReturnsNull()
        {
            // Arrange
            var result = CreateVersionProviderWithAcr(
                enableDynamicInstall: true,
                enableExternalAcrSdkProvider: true,
                externalAcrReturnsNull: true);

            // Act
            var versionInfo = result.VersionProvider.GetVersionInfo();

            // Assert
            Assert.True(result.ExternalAcrVersionProvider.GetVersionInfoCalled);
            Assert.True(result.StorageVersionProvider.GetVersionInfoCalled);
        }

        [Fact]
        public void GetsVersions_FallsBackToStorage_WhenAcrProviderThrows()
        {
            // Arrange
            var result = CreateVersionProviderWithAcr(
                enableDynamicInstall: true,
                enableAcrSdkProvider: true,
                acrThrowsException: true);

            // Act
            var versionInfo = result.VersionProvider.GetVersionInfo();

            // Assert
            Assert.True(result.AcrVersionProvider.GetVersionInfoCalled);
            Assert.True(result.StorageVersionProvider.GetVersionInfoCalled);
        }

        [Fact]
        public void GetsVersions_FallsBackToDirectAcr_WhenExternalAcrAndExternalSdkReturnNull()
        {
            // Arrange
            var result = CreateVersionProviderWithAcr(
                enableDynamicInstall: true,
                enableExternalAcrSdkProvider: true,
                enableExternalSdkProvider: true,
                enableAcrSdkProvider: true,
                externalAcrReturnsNull: true,
                externalSdkReturnsNull: true);

            // Act
            var versionInfo = result.VersionProvider.GetVersionInfo();

            // Assert
            Assert.True(result.ExternalAcrVersionProvider.GetVersionInfoCalled);
            Assert.True(result.ExternalVersionProvider.GetVersionInfoCalled);
            Assert.True(result.AcrVersionProvider.GetVersionInfoCalled);
            Assert.False(result.StorageVersionProvider.GetVersionInfoCalled);
        }

        [Fact]
        public void GetsVersions_FallsBackToCdn_WhenAllProvidersReturnNull()
        {
            // Arrange
            var result = CreateVersionProviderWithAcr(
                enableDynamicInstall: true,
                enableExternalAcrSdkProvider: true,
                enableExternalSdkProvider: true,
                enableAcrSdkProvider: true,
                externalAcrReturnsNull: true,
                externalSdkReturnsNull: true,
                acrThrowsException: true);

            // Act
            var versionInfo = result.VersionProvider.GetVersionInfo();

            // Assert
            Assert.True(result.ExternalAcrVersionProvider.GetVersionInfoCalled);
            Assert.True(result.ExternalVersionProvider.GetVersionInfoCalled);
            Assert.True(result.AcrVersionProvider.GetVersionInfoCalled);
            Assert.True(result.StorageVersionProvider.GetVersionInfoCalled);
        }

        private class TestPythonExternalVersionProvider : PythonExternalVersionProvider
        {
            private readonly bool _returnsNull;

            public TestPythonExternalVersionProvider(
                IOptions<BuildScriptGeneratorOptions> commonOptions, IExternalSdkProvider externalProvider, ILoggerFactory loggerFactory,
                bool returnsNull = false)
                : base(commonOptions, externalProvider, loggerFactory)
            {
                _returnsNull = returnsNull;
            }

            public bool GetVersionInfoCalled { get; private set; }

            public override PlatformVersionInfo GetVersionInfo()
            {
                GetVersionInfoCalled = true;
                if (_returnsNull)
                {
                    return null;
                }

                return PlatformVersionInfo.CreateAvailableViaExternalProvider(
                    new[] { "1.0.0" }, "1.0.0");
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
                new PythonExternalAcrVersionProvider(Options.Create(new BuildScriptGeneratorOptions()), NullLoggerFactory.Instance, new DefaultStandardOutputWriter()),
                new PythonAcrVersionProvider(commonOptions, new OciRegistryClient("https://test.azurecr.io", new TestHttpClientFactory(), NullLoggerFactory.Instance), NullLoggerFactory.Instance),
                NullLogger<PythonVersionProvider>.Instance,
                new DefaultStandardOutputWriter());
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

        private class TestPythonExternalAcrVersionProvider : PythonExternalAcrVersionProvider
        {
            private readonly bool _returnsNull;

            public TestPythonExternalAcrVersionProvider(
                IOptions<BuildScriptGeneratorOptions> options,
                ILoggerFactory loggerFactory,
                IStandardOutputWriter outputWriter,
                bool returnsNull = false)
                : base(options, loggerFactory, outputWriter)
            {
                _returnsNull = returnsNull;
            }

            public bool GetVersionInfoCalled { get; private set; }

            public override PlatformVersionInfo GetVersionInfo()
            {
                GetVersionInfoCalled = true;
                if (_returnsNull)
                {
                    return null;
                }

                return PlatformVersionInfo.CreateAvailableOnAcr(
                    new[] { "3.10.0" }, "3.10.0");
            }
        }

        private class TestPythonAcrVersionProvider : PythonAcrVersionProvider
        {
            private readonly bool _throwsException;

            public TestPythonAcrVersionProvider(
                IOptions<BuildScriptGeneratorOptions> commonOptions,
                OciRegistryClient ociClient,
                ILoggerFactory loggerFactory,
                bool throwsException = false)
                : base(commonOptions, ociClient, loggerFactory)
            {
                _throwsException = throwsException;
            }

            public bool GetVersionInfoCalled { get; private set; }

            public override PlatformVersionInfo GetVersionInfo()
            {
                GetVersionInfoCalled = true;
                if (_throwsException)
                {
                    throw new System.Exception("ACR provider simulated failure");
                }

                return PlatformVersionInfo.CreateAvailableOnAcr(
                    new[] { "3.10.0" }, "3.10.0");
            }
        }

        private class VersionProviderResult
        {
            public IPythonVersionProvider VersionProvider { get; set; }

            public TestPythonOnDiskVersionProvider OnDiskVersionProvider { get; set; }

            public TestPythonSdkStorageVersionProvider StorageVersionProvider { get; set; }

            public TestPythonExternalVersionProvider ExternalVersionProvider { get; set; }

            public TestPythonExternalAcrVersionProvider ExternalAcrVersionProvider { get; set; }

            public TestPythonAcrVersionProvider AcrVersionProvider { get; set; }
        }

        private VersionProviderResult CreateVersionProviderWithAcr(
            bool enableDynamicInstall,
            bool enableExternalSdkProvider = false,
            bool enableExternalAcrSdkProvider = false,
            bool enableAcrSdkProvider = false,
            bool externalAcrReturnsNull = false,
            bool externalSdkReturnsNull = false,
            bool acrThrowsException = false)
        {
            var commonOptions = Options.Create(new BuildScriptGeneratorOptions()
            {
                EnableDynamicInstall = enableDynamicInstall,
                EnableExternalSdkProvider = enableExternalSdkProvider,
                EnableExternalAcrSdkProvider = enableExternalAcrSdkProvider,
                EnableAcrSdkProvider = enableAcrSdkProvider,
            });

            var onDiskProvider = new TestPythonOnDiskVersionProvider();
            var storageProvider = new TestPythonSdkStorageVersionProvider(
                commonOptions,
                new TestHttpClientFactory(),
                NullLoggerFactory.Instance);
            var externalProvider = new TestPythonExternalVersionProvider(
                commonOptions,
                new TestExternalSdkProvider(),
                NullLoggerFactory.Instance,
                returnsNull: externalSdkReturnsNull);
            var externalAcrProvider = new TestPythonExternalAcrVersionProvider(
                commonOptions,
                NullLoggerFactory.Instance,
                new DefaultStandardOutputWriter(),
                returnsNull: externalAcrReturnsNull);
            var acrProvider = new TestPythonAcrVersionProvider(
                commonOptions,
                new OciRegistryClient("https://test.azurecr.io", new TestHttpClientFactory(), NullLoggerFactory.Instance),
                NullLoggerFactory.Instance,
                throwsException: acrThrowsException);
            var versionProvider = new PythonVersionProvider(
                commonOptions,
                onDiskProvider,
                storageProvider,
                externalProvider,
                externalAcrProvider,
                acrProvider,
                NullLogger<PythonVersionProvider>.Instance,
                new DefaultStandardOutputWriter());

            return new VersionProviderResult
            {
                VersionProvider = versionProvider,
                OnDiskVersionProvider = onDiskProvider,
                StorageVersionProvider = storageProvider,
                ExternalVersionProvider = externalProvider,
                ExternalAcrVersionProvider = externalAcrProvider,
                AcrVersionProvider = acrProvider,
            };
        }
    }
}
