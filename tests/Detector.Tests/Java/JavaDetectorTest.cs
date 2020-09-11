// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.Detector.Java;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests.Java
{
    public class JavaDetectorTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;

        public JavaDetectorTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        [Fact]
        public void Detect_ReturnsNull_WhenSourceDirectoryIsEmpty()
        {
            // Arrange
            // No files in source repo
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString())).FullName;
            var repo = new LocalSourceRepo(sourceDir);
            var detector = CreateJavaDetector();
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("java")]
        [InlineData("jsp")]
        public void Detect_ReutrnsResult_WhenRepoHasFileWithSupportedJavaExtensions(string extension)
        {
            // Arrange
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString()))
                .FullName;
            File.WriteAllText(Path.Combine(sourceDir, $"main.{extension}"), "file content here");
            var repo = new LocalSourceRepo(sourceDir);
            var detector = CreateJavaDetector();
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            var javaPlatformDetectorResult = Assert.IsType<JavaPlatformDetectorResult>(result);
            Assert.Equal(JavaConstants.PlatformName, javaPlatformDetectorResult.Platform);
            Assert.False(javaPlatformDetectorResult.UsesMaven);
            Assert.False(javaPlatformDetectorResult.UsesMavenWrapperTool);
            Assert.Null(javaPlatformDetectorResult.PlatformVersion);
        }

        [Theory]
        [InlineData("java")]
        [InlineData("jsp")]
        public void Detect_ReutrnsResult_WhenRepoHasFileWithSupportedJavaExtensionsInNestedDirectories(string extension)
        {
            // Arrange
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString()))
                .FullName;
            var subDir = Directory.CreateDirectory(Path.Combine(sourceDir, Guid.NewGuid().ToString())).FullName;
            File.WriteAllText(Path.Combine(subDir, $"main.{extension}"), "file content here");
            var repo = new LocalSourceRepo(sourceDir);
            var detector = CreateJavaDetector();
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            var javaPlatformDetectorResult = Assert.IsType<JavaPlatformDetectorResult>(result);
            Assert.Equal(JavaConstants.PlatformName, javaPlatformDetectorResult.Platform);
            Assert.False(javaPlatformDetectorResult.UsesMaven);
            Assert.False(javaPlatformDetectorResult.UsesMavenWrapperTool);
            Assert.Null(javaPlatformDetectorResult.PlatformVersion);
        }

        [Fact]
        public void Detect_ReutrnsNull_WhenRepoHasMavenRelatedFilesButNoKnownJavaFileExtensions()
        {
            // Arrange
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString()))
                .FullName;
            File.WriteAllText(Path.Combine(sourceDir, "pom.xml"), "file content here");
            var repo = new LocalSourceRepo(sourceDir);
            var detector = CreateJavaDetector();
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReutrnsResult_WhenRepoHasMavenProjectObjectModelFile()
        {
            // Arrange
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString()))
                .FullName;
            File.WriteAllText(Path.Combine(sourceDir, "app.java"), "file content here");
            File.WriteAllText(Path.Combine(sourceDir, "pom.xml"), "file content here");
            var repo = new LocalSourceRepo(sourceDir);
            var detector = CreateJavaDetector();
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            var javaPlatformDetectorResult = Assert.IsType<JavaPlatformDetectorResult>(result);
            Assert.Equal(JavaConstants.PlatformName, javaPlatformDetectorResult.Platform);
            Assert.True(javaPlatformDetectorResult.UsesMaven);
            Assert.False(javaPlatformDetectorResult.UsesMavenWrapperTool);
            Assert.Null(javaPlatformDetectorResult.PlatformVersion);
        }

        [Fact]
        public void Detect_ReutrnsResult_WhenRepoHasMavenProjectObjectModelFileAndUsesMavenWrapperScript()
        {
            // Arrange
            var sourceDir = Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString()))
                .FullName;
            File.WriteAllText(Path.Combine(sourceDir, "app.java"), "file content here");
            File.WriteAllText(Path.Combine(sourceDir, "pom.xml"), "file content here");
            File.WriteAllText(Path.Combine(sourceDir, "mvnw"), "file content here");
            var repo = new LocalSourceRepo(sourceDir);
            var detector = CreateJavaDetector();
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            var javaPlatformDetectorResult = Assert.IsType<JavaPlatformDetectorResult>(result);
            Assert.Equal(JavaConstants.PlatformName, javaPlatformDetectorResult.Platform);
            Assert.True(javaPlatformDetectorResult.UsesMaven);
            Assert.True(javaPlatformDetectorResult.UsesMavenWrapperTool);
            Assert.Null(javaPlatformDetectorResult.PlatformVersion);
        }

        private DetectorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new DetectorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private JavaDetector CreateJavaDetector()
        {
            return new JavaDetector(NullLogger<JavaDetector>.Instance);
        }
    }
}
