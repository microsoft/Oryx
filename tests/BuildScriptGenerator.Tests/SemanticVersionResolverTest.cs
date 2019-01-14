// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class SemanticVersionResolverTest
    {
        [Fact]
        public void MajorMinorAndPatchVersionsProvided_MatchesExactVersionIfAvailable()
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var supportedVersions = new[] { "1.2.3", "1.2.4", "1.3.0", "2.0.0", "2.3.0" };

            // Act
            var returnedVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(expectedVersion, supportedVersions);

            // Assert;
            Assert.Equal(expectedVersion, returnedVersion);
        }

        [Fact]
        public void MajorMinorAndPatchVersionsProvided_DoesNotMatch_IfExactVersionNotAvailable()
        {
            // Arrange
            var providedVersion = "1.2.3";
            var supportedVersions = new[] { "1.2.2", "1.2.4", "1.3.0", "2.0.0", "2.3.0" };

            // Act
            var returnedVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(providedVersion, supportedVersions);

            // Assert;
            Assert.Null(returnedVersion);
        }

        [Theory]
        [InlineData("1.2")]
        [InlineData("1.2.x")]
        public void MajorAndMinorVersionsProvided_MatchesMajorMinorAndLatestPatchVersion(string providedVersion)
        {
            // Arrange
            var expectedVersion = "1.2.4";
            var supportedVersions = new[] { "1.2.2", "1.2.4", "1.3.0", "1.3.4", "2.0.0", "2.3.0" };

            // Act
            var returnedVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(providedVersion, supportedVersions);

            // Assert;
            Assert.Equal(expectedVersion, returnedVersion);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("1.x")]
        [InlineData("1.x.x")]
        public void MajorVersionProvided_MatchesMajorAndLatestMinorAndPatchVersion(string providedVersion)
        {
            // Arrange
            var expectedVersion = "1.3.4";
            var supportedVersions = new[] { "1.2.2", "1.2.4", "1.3.0", "1.3.4", "2.0.0", "2.3.0" };

            // Act
            var returnedVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(providedVersion, supportedVersions);

            // Assert;
            Assert.Equal(expectedVersion, returnedVersion);
        }

        [Fact]
        public void MinimumVersionSpecified()
        {
            // Arrange
            var supportedVersions = new[] { "1.2.3", "1.2.4" };

            // Act
            var returnedVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(">=1.2.3", supportedVersions);

            // Assert;
            Assert.Equal("1.2.4", returnedVersion);
        }

        [Fact]
        public void SpecificVersionSpecified()
        {
            // Arrange
            var supportedVersions = new[] { "1.2.3", "1.2.4" };

            // Act
            var returnedVersion = SemanticVersionResolver.GetMaxSatisfyingVersion("=1.2.3", supportedVersions);

            // Assert;
            Assert.Equal("1.2.3", returnedVersion);
        }
    }
}