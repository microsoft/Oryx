// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodeVersionProviderTest
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

        private class TestNodeSdkStorageVersionProvider : NodeSdkStorageVersionProvider
        {
            public TestNodeSdkStorageVersionProvider(
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

        private class TestNodeExternalVersionProvider : NodeExternalVersionProvider
        {
            public TestNodeExternalVersionProvider(
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

        private (INodeVersionProvider, TestNodeOnDiskVersionProvider, TestNodeSdkStorageVersionProvider)
            CreateVersionProvider(bool enableDynamicInstall)
        {
            var commonOptions = Options.Create(new BuildScriptGeneratorOptions()
            {
                EnableDynamicInstall = enableDynamicInstall
            });

            var onDiskProvider = new TestNodeOnDiskVersionProvider(commonOptions);
            var storageProvider = new TestNodeSdkStorageVersionProvider(
                commonOptions,
                new TestHttpClientFactory(),
                NullLoggerFactory.Instance);
            var externalProvider = new TestNodeExternalVersionProvider(
                commonOptions,
                new ExternalSdkProvider(NullLogger<ExternalSdkProvider>.Instance),
                NullLoggerFactory.Instance);
            var versionProvider = new NodeVersionProvider(
                commonOptions,
                onDiskProvider,
                storageProvider,
                externalProvider,
                NullLogger<NodeVersionProvider>.Instance);
            return (versionProvider, onDiskProvider, storageProvider);
        }

        private class TestNodeOnDiskVersionProvider : NodeOnDiskVersionProvider
        {
            public TestNodeOnDiskVersionProvider(IOptions<BuildScriptGeneratorOptions> commonOptions)
                : base(commonOptions, NullLogger<NodeOnDiskVersionProvider>.Instance)
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
