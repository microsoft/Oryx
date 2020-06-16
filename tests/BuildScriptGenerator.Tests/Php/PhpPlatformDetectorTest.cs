// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Php
{
    public class PhpPlatformDetectorTest
    {
        [Fact]
        public void Detect_ReturnsNull_WhenSourceDirectoryIsEmpty()
        {
            // Arrange
            var detector = CreatePhpPlatformDetector(supportedPhpVersions: new[] { PhpVersions.Php73Version });
            var repo = new MemorySourceRepo(); // No files in source repo
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReutrnsNull_WhenComposerFileDoesNotExist()
        {
            // Arrange
            var detector = CreatePhpPlatformDetector(supportedPhpVersions: new[] { PhpVersions.Php73Version });
            var repo = new MemorySourceRepo();
            repo.AddFile("foo.php content", "foo.php");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsVersion_FromComposerFile_IfOptionsDoesNotHaveValue()
        {
            // Arrange
            var detector = CreatePhpPlatformDetector(
                supportedPhpVersions: new[] { "7.3.14", "7.2.5", "5.6.0" },
                defaultVersion: "7.3.14",
                new PhpScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            var version = "5.6.0";
            repo.AddFile("{\"require\":{\"php\":\"" + version + "\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("5.6.0", result.PlatformVersion);
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private PhpPlatformDetector CreatePhpPlatformDetector(string[] supportedPhpVersions)
        {
            return CreatePhpPlatformDetector(
                supportedPhpVersions,
                defaultVersion: null,
                new PhpScriptGeneratorOptions());
        }

        private PhpPlatformDetector CreatePhpPlatformDetector(
            string[] supportedPhpVersions,
            string defaultVersion,
            PhpScriptGeneratorOptions options)
        {
            options = options ?? new PhpScriptGeneratorOptions();

            return new PhpPlatformDetector(
                Options.Create(options),
                new TestPhpVersionProvider(supportedPhpVersions, defaultVersion),
                NullLogger<PhpPlatformDetector>.Instance,
                new DefaultStandardOutputWriter());
        }
    }
}
