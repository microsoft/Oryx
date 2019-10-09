// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System.IO;
using Xunit;

namespace Microsoft.Oryx.Common.Extensions
{
    public class FileExtensionsTest : IClassFixture<TestTempDirTestFixture>
    {
        private TestTempDirTestFixture _testDir;
        private string _testDirPath;

        public FileExtensionsTest(TestTempDirTestFixture testFixture)
        {
            _testDir = testFixture;
            _testDirPath = testFixture.RootDirPath;
        }

        [Fact]
        public void SafeWriteAllText_Validate_EmptyPath()
        {
            var contents = "Test content";

            // No failure for empty string
            string.Empty.SafeWriteAllText(contents);
        }

        [Fact]
        public void SafeWriteAllText_Validate_ExistingParentDirectory()
        {
            var contents = "Test content";

            // Existing parent directory, no file
            var outputPath = Path.Combine(_testDirPath, "test.txt");
            outputPath.SafeWriteAllText(contents);
            Assert.True(File.Exists(outputPath));
            Assert.Equal(File.ReadAllText(outputPath), contents);
        }

        [Fact]
        public void SafeWriteAllText_Validate_ExistingFile()
        {
            var contents = "Test content";
            var overwrittenContents = "Overwritten test contents";

            // No failure for empty string
            string.Empty.SafeWriteAllText(contents);

            // Existing parent directory, no file
            var outputPath = Path.Combine(_testDirPath, "test.txt");
            outputPath.SafeWriteAllText(contents);
            Assert.True(File.Exists(outputPath));
            Assert.Equal(File.ReadAllText(outputPath), contents);

            // Existing parent directory, overwrites file
            outputPath.SafeWriteAllText(overwrittenContents);
            Assert.Equal(File.ReadAllText(outputPath), overwrittenContents);
        }

        [Fact]
        public void SafeWriteAllText_Validate_NonExistentParentDirectory()
        {
            var contents = "Test content";

            // Non-existent parent directory
            var outputPath = Path.Combine(_testDirPath, "test.txt");
            var nonExistentDirectory = _testDir.GenerateRandomChildDirPath();
            outputPath = Path.Combine(nonExistentDirectory, "test.txt");
            outputPath.SafeWriteAllText(contents);
            Assert.True(Directory.Exists(nonExistentDirectory));
            Assert.Equal(File.ReadAllText(outputPath), contents);
        }
    }
}
