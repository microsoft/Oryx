// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Common.Tests
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
            return !string.IsNullOrEmpty(dirName)
                && !string.IsNullOrWhiteSpace(dirName)
                && Directory.Exists(_tempDirRoot)
                ? Directory.CreateDirectory(Path.Combine(_tempDirRoot, dirName)).FullName
                : Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString("N"))).FullName;
        }

        [Fact]
        public void FetchDirectoryStructure_ReturnsData_OnlyUpToDefaultMaximumDepth()
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
            Assert.Contains("root.log", result);
            Assert.Contains("temp1.log", result);
            Assert.Contains("temp2", result);
            Assert.DoesNotContain("temp2.log", result);
            Assert.DoesNotContain("temp3.log", result);
        }

        [Fact]
        public void FetchDirectoryStructure_ReturnsData_OnlyUpToDefaultMaximumFileCount()
        {
            // Arrange

            var level1Dir = CreateNewDir("tmp1");
            var level2Dir = CreateNewDir(Path.Combine(level1Dir, "tmp2"));
            var level2Dir2 = CreateNewDir(Path.Combine(level1Dir, "tmp22"));
            var level3Dir = CreateNewDir(Path.Combine(level2Dir, "tmp3"));
            var levelFile = File.Create(Path.Combine(level1Dir, "tmp1.log"));
            var level2File = File.Create(Path.Combine(level2Dir, "tmp2.log"));
            var level3File = File.Create(Path.Combine(level3Dir, "tmp3.log"));
            var level1Dir2 = CreateNewDir("tmp11");

            for (int i = 0; i < 1000; i++)
            {
                var fileName = i.ToString().PadLeft(4, '0') + ".log";
                var level2files = File.Create(Path.Combine(level2Dir, fileName));
            }

            for (int i = 0; i < 100; i++)
            {
                var fileName = i.ToString().PadLeft(4, '0') + ".txt";
                var level2files = File.Create(Path.Combine(level1Dir, fileName));
            }

            // Act
            var result = OryxDirectoryStructureHelper.GetDirectoryStructure(level1Dir);

            // Assert
            Assert.Contains("0000.txt", result);
            Assert.Contains("0099.txt", result);
            Assert.Contains("0898.log", result);
            Assert.DoesNotContain("0899.log", result);
            Assert.Contains("tmp1.log", result);
            Assert.DoesNotContain("tmp2.log", result);
            Assert.DoesNotContain("tmp3.log", result);
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