// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DefaultSourceRepoProviderTest : IClassFixture<DefaultSourceRepoProviderTest.TestFixture>
    {
        private readonly string _tempDirRootPath;

        public DefaultSourceRepoProviderTest(TestFixture fixutre)
        {
            _tempDirRootPath = fixutre.TempDirPath;
        }

        [Fact]
        public void IntermediateDir_IsNotUsed_WhenOptedTo()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var appDir = Path.Combine(_tempDirRootPath, $"app-{guid}");
            var tempDir = Path.Combine(_tempDirRootPath, $"temp-{guid}");
            Directory.CreateDirectory(appDir);
            Directory.CreateDirectory(tempDir);
            var options = new BuildScriptGeneratorOptions
            {
                Inline = true,
                SourceDir = appDir,
                TempDir = tempDir
            };
            var provider = GetSourceRepoProvider(options);

            // Act
            var sourceRepo = provider.GetSourceRepo();

            // Assert
            Assert.Equal(appDir, sourceRepo.RootPath);
            Assert.False(Directory.Exists(Path.Combine(tempDir, "IntermediateDir")));
        }

        [Fact]
        public void IntermediateDir_IsNotUsed_WhenOptedTo_AndIntermediateDirOptionIsProvided()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var appDir = Path.Combine(_tempDirRootPath, $"app-{guid}");
            var tempDir = Path.Combine(_tempDirRootPath, $"temp-{guid}");
            var intermediateDir = Path.Combine(_tempDirRootPath, $"intermediate-{guid}");
            Directory.CreateDirectory(appDir);
            Directory.CreateDirectory(tempDir);
            var options = new BuildScriptGeneratorOptions
            {
                Inline = true,
                IntermediateDir = intermediateDir,
                SourceDir = appDir,
                TempDir = tempDir
            };
            var provider = GetSourceRepoProvider(options);

            // Act
            var sourceRepo = provider.GetSourceRepo();

            // Assert
            Assert.Equal(appDir, sourceRepo.RootPath);
            Assert.False(Directory.Exists(intermediateDir));
        }

        [Fact]
        public void ByDefault_CopiesSourceDirContent_ToTempDirectoryIntermediateDir()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var appDir = Path.Combine(_tempDirRootPath, $"app-{guid}");
            var tempDir = Path.Combine(_tempDirRootPath, $"temp-{guid}");
            Directory.CreateDirectory(appDir);
            Directory.CreateDirectory(tempDir);
            var options = new BuildScriptGeneratorOptions
            {
                SourceDir = appDir,
                TempDir = tempDir,
            };
            var provider = GetSourceRepoProvider(options);

            // Create content in app's directory
            var srcDirName = Guid.NewGuid().ToString();
            var srcDirPath = Directory.CreateDirectory(Path.Combine(appDir, srcDirName));
            var file1Path = Path.Combine(srcDirPath.FullName, "file1.txt");
            File.WriteAllText(file1Path, "file1.txt content");

            var expected = Path.Combine(tempDir, "IntermediateDir");
            var expectedFile = Path.Combine(expected, srcDirName, "file1.txt");

            // Act
            var sourceRepo = provider.GetSourceRepo();

            // Assert
            Assert.Equal(expected, sourceRepo.RootPath);
            Assert.True(File.Exists(expectedFile));
        }

        [Fact]
        public void CopiesSourceDirContent_ToCustom_IntermediateDir()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var appDir = Path.Combine(_tempDirRootPath, $"app-{guid}");
            var tempDir = Path.Combine(_tempDirRootPath, $"temp-{guid}");
            var intermediateDir = Path.Combine(_tempDirRootPath, $"intermediate-{guid}");
            Directory.CreateDirectory(appDir);
            Directory.CreateDirectory(tempDir);
            var options = new BuildScriptGeneratorOptions
            {
                SourceDir = appDir,
                TempDir = tempDir,
                IntermediateDir = intermediateDir
            };
            var provider = GetSourceRepoProvider(options);

            // Create content in app's directory
            var srcDirName = Guid.NewGuid().ToString();
            var srcDirPath = Directory.CreateDirectory(Path.Combine(appDir, srcDirName));
            var file1Path = Path.Combine(srcDirPath.FullName, "file1.txt");
            File.WriteAllText(file1Path, "file1.txt content");

            var expected = intermediateDir;
            var expectedFile = Path.Combine(expected, srcDirName, "file1.txt");

            // Act
            var sourceRepo = provider.GetSourceRepo();

            // Assert
            Assert.Equal(expected, sourceRepo.RootPath);
            Assert.True(File.Exists(expectedFile));
        }

        [Fact]
        public void CopiesSourceDirContent_Recursively()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var appDir = Path.Combine(_tempDirRootPath, $"app-{guid}");
            var tempDir = Path.Combine(_tempDirRootPath, $"temp-{guid}");
            Directory.CreateDirectory(appDir);
            Directory.CreateDirectory(tempDir);
            var options = new BuildScriptGeneratorOptions
            {
                SourceDir = appDir,
                TempDir = tempDir,
            };
            var provider = GetSourceRepoProvider(options);

            // Create content in app's directory
            var srcDirName = Guid.NewGuid().ToString();
            var srcDirPath = Directory.CreateDirectory(Path.Combine(appDir, srcDirName));
            var srcSubDirPath = Directory.CreateDirectory(Path.Combine(srcDirPath.FullName, "subDir1"));
            var file1Path = Path.Combine(srcDirPath.FullName, "file1.txt");
            var file2Path = Path.Combine(srcSubDirPath.FullName, "file2.txt");
            File.WriteAllText(file1Path, "file1.txt content");
            File.WriteAllText(file2Path, "file2.txt content");

            var expected = Path.Combine(tempDir, "IntermediateDir");
            var expectedFile = Path.Combine(expected, srcDirName, "subDir1", "file2.txt");

            // Act
            var sourceRepo = provider.GetSourceRepo();

            // Assert
            Assert.Equal(expected, sourceRepo.RootPath);
            Assert.True(File.Exists(expectedFile));
        }

        private ISourceRepoProvider GetSourceRepoProvider(BuildScriptGeneratorOptions options)
        {
            return new DefaultSourceRepoProvider(
                Options.Create(options),
                NullLogger<DefaultSourceRepoProvider>.Instance);
        }

        public class TestFixture : IDisposable
        {
            public TestFixture()
            {
                TempDirPath = Path.Combine(
                    Path.GetTempPath(),
                    nameof(DefaultSourceRepoProviderTest),
                    "Temp");

                Directory.CreateDirectory(TempDirPath);
            }

            public string TempDirPath { get; }

            public void Dispose()
            {
                if (Directory.Exists(TempDirPath))
                {
                    try
                    {
                        Directory.Delete(TempDirPath, recursive: true);
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
