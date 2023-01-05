// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests
{
    public class LocalSourceRepoTest : IClassFixture<LocalSourceRepoTest.SourceRepoTestFixture>
    {
        private readonly string _rootDirPath;

        public LocalSourceRepoTest(SourceRepoTestFixture fixture)
        {
            _rootDirPath = fixture.RootDirPath;
        }

        [Fact]
        public void RootPath_ReturnsRootOfTheSourceDirectory()
        {
            // Arrange & Act
            var sourceRepo = new LocalSourceRepo(_rootDirPath, NullLoggerFactory.Instance);

            // Assert
            Assert.Equal(_rootDirPath, sourceRepo.RootPath);
        }

        [Theory]
        [InlineData("root1.txt")]
        [InlineData("a", "a1.txt")]
        [InlineData("a", "aa", "aa1.txt")]
        public void FileExists_DoesRelativePathLookupForFiles(params string[] paths)
        {
            // Arrange
            var sourceRepo = new LocalSourceRepo(_rootDirPath, NullLoggerFactory.Instance);

            // Act
            var exists = sourceRepo.FileExists(paths);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public void ReadFile_ReturnsConentOfTheFileRequested()
        {
            // Arrange-1
            var sourceRepo = new LocalSourceRepo(_rootDirPath, NullLoggerFactory.Instance);

            // Act-1
            var exists = sourceRepo.FileExists("a", "aa", "aa1.txt");

            // Assert-1
            Assert.True(exists);

            // Arrange-2
            var expected = $"file in {Path.Combine(_rootDirPath, "a", "aa")}";

            // Act-2
            var content = sourceRepo.ReadFile("a", "aa", "aa1.txt");

            // Assert-2
            Assert.Equal(expected, content);
        }

        [Fact]
        public void GetFilesWithExtension_ReturnsFilesAtRootDirectoryOnly_IfSubDirectorySearchIsFalse()
        {
            // Arrange
            var expected = Path.Combine(_rootDirPath, "root1.txt");
            var sourceRepo = new LocalSourceRepo(_rootDirPath, NullLoggerFactory.Instance);

            // Act
            var files = sourceRepo.EnumerateFiles("*.txt", searchSubDirectories: false);

            // Assert
            Assert.NotNull(files);
            var file = Assert.Single(files);
            Assert.Equal(expected, file);
        }

        [Fact]
        public void GetFilesWithExtension_ReturnsFilesAtSpecifiedSubDirectoryOnly_AndSubDirectorySearchIsFalse()
        {
            // Arrange
            var expected = Path.Combine(_rootDirPath, "a", "a1.txt");
            var sourceRepo = new LocalSourceRepo(_rootDirPath, NullLoggerFactory.Instance);

            // Act
            var files = sourceRepo.EnumerateFiles(
                "*.txt",
                searchSubDirectories: false,
                subDirectoryToSearchUnder: "a");

            // Assert
            Assert.NotNull(files);
            var file = Assert.Single(files);
            Assert.Equal(expected, file);
        }

        [Fact]
        public void GetFilesWithExtension_ReturnsFilesAtSpecifiedSubDirectoryAndItsSubDirectories_IfSubDirectorySearchIsTrue()
        {
            // Arrange
            var expected1 = Path.Combine(_rootDirPath, "a", "a1.txt");
            var expected2 = Path.Combine(_rootDirPath, "a", "aa", "aa1.txt");
            var sourceRepo = new LocalSourceRepo(_rootDirPath, NullLoggerFactory.Instance);

            // Act
            var files = sourceRepo.EnumerateFiles(
                "*.txt",
                searchSubDirectories: true,
                subDirectoryToSearchUnder: "a");

            // Assert
            Assert.NotNull(files);
            Assert.Equal(2, files.Count());
            Assert.Contains(expected1, files);
            Assert.Contains(expected2, files);
        }

        [Fact]
        public void GetFilesWithExtension_ReturnsFilesAtAllDirectories_IfSubDirectorySearchIsTrue()
        {
            // Arrange
            var expected1 = Path.Combine(_rootDirPath, "a", "a1.txt");
            var expected2 = Path.Combine(_rootDirPath, "a", "aa", "aa1.txt");
            var expected3 = Path.Combine(_rootDirPath, "b", "b1.txt");
            var expected4 = Path.Combine(_rootDirPath, "b", "bb", "bb1.txt");
            var expected5 = Path.Combine(_rootDirPath, "root1.txt");
            var sourceRepo = new LocalSourceRepo(_rootDirPath, NullLoggerFactory.Instance);

            // Act
            var files = sourceRepo.EnumerateFiles("*.txt", searchSubDirectories: true);

            // Assert
            Assert.NotNull(files);
            Assert.Equal(5, files.Count());
            Assert.Contains(expected1, files);
            Assert.Contains(expected2, files);
            Assert.Contains(expected3, files);
            Assert.Contains(expected4, files);
            Assert.Contains(expected5, files);
        }

        public class SourceRepoTestFixture : IDisposable
        {
            public SourceRepoTestFixture()
            {
                RootDirPath = Path.Combine(Path.GetTempPath(), "oryxtests", Guid.NewGuid().ToString());

                // /root/
                //      root1.txt
                //      root1.xml
                //      a/
                //          a1.txt
                //          a1.xml
                //          aa/
                //              aa1.txt
                //              aa1.xml
                //      b/
                //          b1.txt
                //          b1.xml
                //          bb/
                //              bb1.txt
                //              bb1.xml
                Directory.CreateDirectory(RootDirPath);
                File.WriteAllText(Path.Combine(RootDirPath, "root1.txt"), "file content");
                File.WriteAllText(Path.Combine(RootDirPath, "root1.xml"), "file content");

                var aDir = Path.Combine(RootDirPath, "a");
                Directory.CreateDirectory(aDir);
                File.WriteAllText(Path.Combine(aDir, "a1.txt"), $"file in {aDir}");
                File.WriteAllText(Path.Combine(aDir, "a1.xml"), $"file in {aDir}");

                var aaDir = Path.Combine(aDir, "aa");
                Directory.CreateDirectory(aaDir);
                File.WriteAllText(Path.Combine(aaDir, "aa1.txt"), $"file in {aaDir}");
                File.WriteAllText(Path.Combine(aaDir, "aa1.xml"), $"file in {aaDir}");

                var bDir = Path.Combine(RootDirPath, "b");
                Directory.CreateDirectory(bDir);
                File.WriteAllText(Path.Combine(bDir, "b1.txt"), $"file in {bDir}");
                File.WriteAllText(Path.Combine(bDir, "b1.xml"), $"file in {bDir}");

                var bbDir = Path.Combine(bDir, "bb");
                Directory.CreateDirectory(bbDir);
                File.WriteAllText(Path.Combine(bbDir, "bb1.txt"), $"file in {bbDir}");
                File.WriteAllText(Path.Combine(bbDir, "bb1.xml"), $"file in {bbDir}");
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
