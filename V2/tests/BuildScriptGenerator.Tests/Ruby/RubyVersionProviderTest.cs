// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Ruby;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Ruby
{
    public class RubyVersionProviderTest
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

        private class TestRubySdkStorageVersionProvider : RubySdkStorageVersionProvider
        {
            public TestRubySdkStorageVersionProvider(
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

        private (IRubyVersionProvider, TestRubyOnDiskVersionProvider, TestRubySdkStorageVersionProvider)
            CreateVersionProvider(bool enableDynamicInstall)
        {
            var commonOptions = Options.Create(new BuildScriptGeneratorOptions()
            {
                EnableDynamicInstall = enableDynamicInstall
            });

            var onDiskProvider = new TestRubyOnDiskVersionProvider();
            var storageProvider = new TestRubySdkStorageVersionProvider(
                commonOptions,
                new TestHttpClientFactory(),
                NullLoggerFactory.Instance);
            var versionProvider = new RubyVersionProvider(
                commonOptions,
                onDiskProvider,
                storageProvider,
                NullLogger<RubyVersionProvider>.Instance);
            return (versionProvider, onDiskProvider, storageProvider);
        }

        private class TestRubyOnDiskVersionProvider : RubyOnDiskVersionProvider
        {
            public TestRubyOnDiskVersionProvider()
                : base(NullLogger<RubyOnDiskVersionProvider>.Instance)
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
