// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
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
            var supportedVersions = new[] { "1.2.2", "1.2.4", "1.3.0", "1.3.4", "2.0.0", "2.3.0", "3.1" };

            // Act
            var returnedVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(providedVersion, supportedVersions);

            // Assert;
            Assert.Equal(expectedVersion, returnedVersion);
        }

        [Theory]
        [InlineData("1", "1.3.4")]
        [InlineData("2", "2.3.0")]
        [InlineData("3", "3.1")]
        public void MajorVersionProvided_MatchesMajorAndLatestMinorVersion(string providedVersion, string expectedVersion)
        {
            // Arrange
            var supportedVersions = new[] { "1.2.2", "1.2.4", "1.3.0", "1.3.4", "2.0.0", "2.3.0", "3.1" };

            // Act
            var returnedVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(providedVersion, supportedVersions);

            // Assert;
            // test will fail for 3-rd test case as we don't have patching version in the maximum supported version
            Assert.Equal(expectedVersion, returnedVersion);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("1.x")]
        [InlineData("1.x.x")]
        public void VersionProvided_MatchesMajorAndLatestMinorVersion(string providedVersion)
        {
            // Arrange
            var expectedVersion = "1.3";
            var supportedVersions = new[] { "1.2", "1.2", "1.3", "1.3", "2", "2.3" };

            // Act
            var returnedVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(providedVersion, supportedVersions);

            // Assert;
            // all will fail as patch version doesn't exist in any of the supported versions
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

        [Theory]
        [InlineData("1")]
        [InlineData("1.2")]
        [InlineData("1.3.4")]
        [InlineData("2")]
        [InlineData("4.5")]
        public void CompareVersionsTest(string providedVersion)
        {
            // Arrange
            var supportedVersions = new[] { "1.2.3", "2", "2.1", "2.1.1" };
            List<int> comparisonResult = new List<int>();

            // Act
            foreach (var version in supportedVersions)
            {
                comparisonResult.Add(SemanticVersionResolver.CompareVersions(providedVersion, version));
            }

            // Assert
            /* Testcase: "1"
             * comparisonResult = {int.MinValue, int.MinValue, int.MinValue, int.MinValue}
             * Testcase: "1.2" 
             * comparisonResult = {-1, int.MinValue, -1 , -1}
             * Testcase: "1.3.4" 
             * comparisonResult = {-1, int.MinValue, -1 , -1}
             * Testcase: "2" 
             * comparisonResult = {int.MinValue, int.MinValue, int.MinValue, int.MinValue}
             * Testcase: "4.5" 
             * comparisonResult = {-1, int.MinValue, -1 , -1}
             */

            Assert.NotEqual(comparisonResult[0], int.MinValue);
            Assert.NotEqual(comparisonResult[1], int.MinValue);
            Assert.NotEqual(comparisonResult[2], int.MinValue);
            Assert.NotEqual(comparisonResult[3], int.MinValue);
        }
    }
}