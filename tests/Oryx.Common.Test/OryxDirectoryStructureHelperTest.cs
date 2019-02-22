// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System;
using System.IO;
using Xunit;

namespace Microsoft.Oryx.Common.Test
{
    public class OryxDirectoryStructureHelperTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;
        public OryxDirectoryStructureHelperTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }
        private string CreateNewDir(string dirName)
        {
            return !string.IsNullOrEmpty(dirName) && !string.IsNullOrWhiteSpace(dirName)
                ? Directory.CreateDirectory(Path.Combine(_tempDirRoot, dirName)).FullName
                : Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString("N"))).FullName;
        }

        [Fact]
        public void FetchDirectoryStructure_ReturnsData_OnlyUntilDefaultMaximumDepth()
        {
            // Arrange
            var level1Dir = CreateNewDir("temp1");
            var level2Dir = CreateNewDir(Path.Combine(level1Dir, "temp2"));
            var level3Dir = CreateNewDir(Path.Combine(level2Dir, "temp3"));
            var levelRootFile = File.Create(Path.Combine(_tempDirRoot, "root.log"));
            var levelFile = File.Create(Path.Combine(level1Dir, "temp1.log"));
            var level2File = File.Create(Path.Combine(level2Dir, "temp2.log"));
            var level3File = File.Create(Path.Combine(level3Dir, "temp3.log"));

            // Act
            var result = OryxDirectoryStructureHelper.GetDirectoryStructure(_tempDirRoot);

            // Assert
            Assert.Contains("root.log",result);
            Assert.Contains("temp1.log", result);
            Assert.Contains("temp2", result);
            Assert.DoesNotContain("temp2.log", result);
            Assert.DoesNotContain("temp3.log", result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public void FetchDirectoryStructure_ReturnsData_When_PathInvalid(string sourceDirPath)
        {
            // Act
            var result = OryxDirectoryStructureHelper.GetDirectoryStructure(sourceDirPath);

            // Assert
            Assert.Contains(string.Empty, result);
        }
    }
}
