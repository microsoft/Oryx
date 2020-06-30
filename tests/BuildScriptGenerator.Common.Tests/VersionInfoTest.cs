// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Xunit;
using Microsoft.Oryx.Tests.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.Common.Tests
{
    public class VersionInfoTest : IClassFixture<TestTempDirTestFixture>
    {
        public VersionInfoTest(TestTempDirTestFixture testFixture)
        {
        }

        [Theory]
        [InlineData("3.9.0b1", "3.9.0-b1")]
        [InlineData("3.8.0-b3", "3.8.0-b3")]
        [InlineData("12.14.0", "12.14.0")]
        [InlineData("3.9.0b1.23", "3.9.0-b1.23")]
        [InlineData("5.0.0-preview.2.20160.6", "5.0.0-preview.2.20160.6")]
        public void VersionInfo_ConvertDisplayVersionToSemanticVersion(string displayVersion, string expectedVersion)
        {
            // Act
            VersionInfo versionInfo = new VersionInfo(displayVersion);

            // Assert
            Assert.Equal(expectedVersion, versionInfo.semanticVersion.ToString());
        }

        [Theory]
        [InlineData("3.889")]
        public void VersionInfo_InvalidDisplayVersion_ThrowsArgumentException(string displayVersion)
        {
            
            // Assert
            Assert.Throws<ArgumentException>(() => { new VersionInfo(displayVersion); });
        }

        [Theory]
        [InlineData("3.8.0", "3.9.0-b1")]
        [InlineData("3.8.0b3", "3.8.0b4")]
        [InlineData("10.1.0", "12.11.1")]
        [InlineData("4.8.7", "10.1.0")]
        [InlineData("5.0.0-preview.1.20120.5", "5.0.0-preview.4.20251.6")]
        [InlineData("6.11.0", "14.0.0")]
        public void VersionInfo_CompareToIsCorrectOrder(string displayVersion1, string displayVersion2)
        {
            // Act
            VersionInfo versionInfo1 = new VersionInfo(displayVersion1);
            VersionInfo versionInfo2 = new VersionInfo(displayVersion2);
            int result = versionInfo1.CompareTo(versionInfo2);

            // Assert
            Assert.Equal(-1, result);
        }
    }
}
