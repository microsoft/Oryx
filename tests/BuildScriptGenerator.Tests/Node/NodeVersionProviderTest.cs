// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodeVersionProviderTest : IClassFixture<NodeVersionProviderTest.TestFixture>
    {
        private readonly string _rootDirPath;

        public NodeVersionProviderTest(TestFixture testFixture)
        {
            _rootDirPath = testFixture.RootDirPath;
        }

        [Fact]
        public void NodeVersions_ReturnsOnlyVersionsWithSemanticVersioning()
        {
            // Arrange
            var provider = GetNodeVersionProvider();

            // Act
            var nodeVersions = provider.SupportedNodeVersions;

            // Assert
            Assert.NotNull(nodeVersions);
            Assert.Equal(2, nodeVersions.Count());
            Assert.Contains("1.0.0", nodeVersions);
            Assert.Contains("100.200.300", nodeVersions);
        }

        [Fact]
        public void NpmVersions_ReturnsOnlyVersionsWithSemanticVersioning()
        {
            // Arrange
            var provider = GetNodeVersionProvider();

            // Act
            var nodeVersions = provider.SupportedNpmVersions;

            // Assert
            Assert.NotNull(nodeVersions);
            Assert.Equal(2, nodeVersions.Count());
            Assert.Contains("1.0.0", nodeVersions);
            Assert.Contains("100.200.300", nodeVersions);
        }

        private NodeVersionProvider GetNodeVersionProvider()
        {
            var options = new NodeScriptGeneratorOptions
            {
                InstalledNodeVersionsDir = _rootDirPath,
                InstalledNpmVersionsDir = _rootDirPath,
            };
            var commonOptions = Options.Create(new BuildScriptGeneratorOptions());
            return new NodeVersionProvider(
                Options.Create(options),
                new TestEnvironment(),
                new NodePlatformInstaller(commonOptions, new TestEnvironment(), new TestHttpClientFactory()));
        }

        public class TestFixture : IDisposable
        {
            public TestFixture()
            {
                RootDirPath = Path.Combine(Path.GetTempPath(), "oryxtests", Guid.NewGuid().ToString());

                Directory.CreateDirectory(RootDirPath);
                Directory.CreateDirectory(Path.Combine(RootDirPath, "1.0.0"));
                Directory.CreateDirectory(Path.Combine(RootDirPath, "100.200.300"));
                Directory.CreateDirectory(Path.Combine(RootDirPath, "latest"));
                Directory.CreateDirectory(Path.Combine(RootDirPath, "lts"));

                // Only top directories are to be searched for versions, for example the directory '1.0.1' should be
                // ignored here.
                Directory.CreateDirectory(Path.Combine(RootDirPath, "1.0.0", "1.0.1"));

            }

            public string RootDirPath { get; }

            public void Dispose()
            {
                if (Directory.Exists(RootDirPath))
                {
                    try
                    {
                        Directory.Delete(RootDirPath, recursive: true);
                    }
                    catch
                    {
                        // Do not throw in dispose
                    }
                }
            }
        }
    }
}
