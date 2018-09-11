// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace BuildScriptGenerator.Tests.UnitTests
{
    using Microsoft.Extensions.Options;
    using Microsoft.Oryx.BuildScriptGenerator.Node;
    using Xunit;

    public class NodeVersionResolverTest
    {
        [Fact]
        public void SimpleSupportedNodeVersionSpecified()
        {
            // Arrange
            const string version = "1.2.3";
            var supportedVersions = new[] { version };
            var nodeVersionResolver = CreateNodeVersionResolver(supportedVersions, supportedNpmVersions: null);

            // Act
            var returnedVersion = nodeVersionResolver.GetSupportedNodeVersion(version);

            // Assert;
            Assert.Equal(version, returnedVersion);
        }

        [Fact]
        public void SimpleUnsupportedNodeVersionSpecified()
        {
            // Arrange
            const string supportedVersion = "1.2.3";
            var supportedVersions = new[] { supportedVersion };
            var nodeVersionResolver = CreateNodeVersionResolver(supportedVersions, supportedNpmVersions: null);

            // Act
            var returnedVersion = nodeVersionResolver.GetSupportedNodeVersion("1.2.4");

            // Assert;
            Assert.Null(returnedVersion);
        }

        [Fact]
        public void MinimumNodeVersionSpecified()
        {
            // Arrange
            var supportedVersions = new[] { "1.2.3", "1.2.4" };
            var nodeVersionResolver = CreateNodeVersionResolver(supportedVersions, supportedNpmVersions: null);

            // Act
            var returnedVersion = nodeVersionResolver.GetSupportedNodeVersion(">=1.2.3");

            // Assert;
            Assert.Equal("1.2.4", returnedVersion);
        }

        [Fact]
        public void SpecificNodeVersionSpecified()
        {
            // Arrange
            var supportedVersions = new[] { "1.2.3", "1.2.4" };
            var nodeVersionResolver = CreateNodeVersionResolver(supportedVersions, supportedNpmVersions: null);

            // Act
            var returnedVersion = nodeVersionResolver.GetSupportedNodeVersion("=1.2.3");

            // Assert;
            Assert.Equal("1.2.3", returnedVersion);
        }

        [Fact]
        public void SimpleSupportedNpmVersionSpecified()
        {
            // Arrange
            const string version = "1.2.3";
            var supportedVersions = new[] { version };
            var nodeVersionResolver = CreateNodeVersionResolver(supportedNodeVersions: null, supportedVersions);

            // Act
            var returnedVersion = nodeVersionResolver.GetSupportedNpmVersion(version);

            // Assert;
            Assert.Equal(version, returnedVersion);
        }

        [Fact]
        public void SimpleUnsupportedNpmVersionSpecified()
        {
            // Arrange
            const string supportedVersion = "1.2.3";
            var supportedVersions = new[] { supportedVersion };
            var nodeVersionResolver = CreateNodeVersionResolver(supportedNodeVersions: null, supportedVersions);

            // Act
            var returnedVersion = nodeVersionResolver.GetSupportedNpmVersion("1.2.4");

            // Assert;
            Assert.Null(returnedVersion);
        }

        [Fact]
        public void MinimumNpmVersionSpecified()
        {
            // Arrange
            var supportedVersions = new[] { "1.2.3", "1.2.4" };
            var nodeVersionResolver = CreateNodeVersionResolver(supportedNodeVersions: null, supportedVersions);

            // Act
            var returnedVersion = nodeVersionResolver.GetSupportedNpmVersion(">=1.2.3");

            // Assert;
            Assert.Equal("1.2.4", returnedVersion);
        }

        [Fact]
        public void SpecificNpmVersionSpecified()
        {
            // Arrange
            var supportedVersions = new[] { "1.2.3", "1.2.4" };
            var nodeVersionResolver = CreateNodeVersionResolver(supportedNodeVersions: null, supportedVersions);

            // Act
            var returnedVersion = nodeVersionResolver.GetSupportedNpmVersion("=1.2.3");

            // Assert;
            Assert.Equal("1.2.3", returnedVersion);
        }

        private INodeVersionResolver CreateNodeVersionResolver(
            string[] supportedNodeVersions,
            string[] supportedNpmVersions)
        {
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            nodeScriptGeneratorOptions.SupportedNodeVersions = supportedNodeVersions;
            nodeScriptGeneratorOptions.SupportedNpmVersions = supportedNpmVersions;

            return new NodeVersionResolver(Options.Create(nodeScriptGeneratorOptions));
        }
    }
}