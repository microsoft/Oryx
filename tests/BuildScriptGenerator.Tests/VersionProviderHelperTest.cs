// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class VersionProviderHelperTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;

        public VersionProviderHelperTest(TestTempDirTestFixture tempDirFixture)
        {
            _tempDirRoot = tempDirFixture.RootDirPath;
        }

        [Fact]
        public void GetVersionsFromDirectory_IgnoresMalformedVersionStrings()
        {
            // Arrange
            var expectedVersion = "1.0.0";
            CreateSubDirectory(expectedVersion);
            CreateSubDirectory("2.0b"); // Invalid SemVer string

            // Act
            var versions = VersionProviderHelper.GetVersionsFromDirectory(_tempDirRoot);

            // Assert
            Assert.Single(versions, expectedVersion);
        }

        [Fact]
        public void GetMajorMinorVersionsFromDirectory_IgnoresMalformedVersionStrings()
        {
            // Arrange
            var expectedVersion = "1.16";
            CreateSubDirectory(expectedVersion);
            CreateSubDirectory("2.0b"); // Invalid Major.Minor version
            CreateSubDirectory("1.2.3"); // Invalid Major.Minor version

            // Act
            var versions = VersionProviderHelper.GetMajorMinorVersionsFromDirectory(_tempDirRoot);

            // Assert
            Assert.Single(versions, expectedVersion);
        }

        private void CreateSubDirectory(string name)
        {
            Directory.CreateDirectory(Path.Combine(_tempDirRoot, name));
        }
    }
}
