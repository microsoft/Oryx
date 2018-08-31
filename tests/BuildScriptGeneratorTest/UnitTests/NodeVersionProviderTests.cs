// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace BuildScriptGeneratorTest.UnitTests
{
    using Microsoft.Oryx.BuildScriptGenerator.Node;
    using Xunit;

    public class NodeVersionProviderTests
    {
        [Fact]
        public void SimpleSupportedNodeVersionSpecified()
        {
            // Arrange
            const string version = "1.2.3";
            var supportedVersions = new[] { version };
            var nodeVersionProvider = new NodeVersionProvider(supportedVersions, null);

            // Act
            var returnedVersion = nodeVersionProvider.GetSupportedNodeVersion(version);

            // Assert;
            Assert.Equal(version, returnedVersion);
        }

        [Fact]
        public void SimpleUnsupportedNodeVersionSpecified()
        {
            // Arrange
            const string supportedVersion = "1.2.3";
            var supportedVersions = new[] { supportedVersion };
            var nodeVersionProvider = new NodeVersionProvider(supportedVersions, null);

            // Act
            var returnedVersion = nodeVersionProvider.GetSupportedNodeVersion("1.2.4");

            // Assert;
            Assert.Null(returnedVersion);
        }

        [Fact]
        public void MinimumNodeVersionSpecified()
        {
            // Arrange
            var supportedVersions = new[] { "1.2.3", "1.2.4" };
            var nodeVersionProvider = new NodeVersionProvider(supportedVersions, null);

            // Act
            var returnedVersion = nodeVersionProvider.GetSupportedNodeVersion(">=1.2.3");

            // Assert;
            Assert.Equal("1.2.4", returnedVersion);
        }

        [Fact]
        public void SpecificNodeVersionSpecified()
        {
            // Arrange
            var supportedVersions = new[] { "1.2.3", "1.2.4" };
            var nodeVersionProvider = new NodeVersionProvider(supportedVersions, null);

            // Act
            var returnedVersion = nodeVersionProvider.GetSupportedNodeVersion("=1.2.3");

            // Assert;
            Assert.Equal("1.2.3", returnedVersion);
        }

        [Fact]
        public void SimpleSupportedNpmVersionSpecified()
        {
            // Arrange
            const string version = "1.2.3";
            var supportedVersions = new[] { version };
            var nodeVersionProvider = new NodeVersionProvider(null, supportedVersions);

            // Act
            var returnedVersion = nodeVersionProvider.GetSupportedNpmVersion(version);

            // Assert;
            Assert.Equal(version, returnedVersion);
        }

        [Fact]
        public void SimpleUnsupportedNpmVersionSpecified()
        {
            // Arrange
            const string supportedVersion = "1.2.3";
            var supportedVersions = new[] { supportedVersion };
            var nodeVersionProvider = new NodeVersionProvider(null, supportedVersions);

            // Act
            var returnedVersion = nodeVersionProvider.GetSupportedNpmVersion("1.2.4");

            // Assert;
            Assert.Null(returnedVersion);
        }

        [Fact]
        public void MinimumNpmVersionSpecified()
        {
            // Arrange
            var supportedVersions = new[] { "1.2.3", "1.2.4" };
            var nodeVersionProvider = new NodeVersionProvider(null, supportedVersions);

            // Act
            var returnedVersion = nodeVersionProvider.GetSupportedNpmVersion(">=1.2.3");

            // Assert;
            Assert.Equal("1.2.4", returnedVersion);
        }

        [Fact]
        public void SpecificNpmVersionSpecified()
        {
            // Arrange
            var supportedVersions = new[] { "1.2.3", "1.2.4" };
            var nodeVersionProvider = new NodeVersionProvider(null, supportedVersions);

            // Act
            var returnedVersion = nodeVersionProvider.GetSupportedNpmVersion("=1.2.3");

            // Assert;
            Assert.Equal("1.2.3", returnedVersion);
        }
    }
}