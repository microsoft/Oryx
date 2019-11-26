// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Php
{
    public class PhpLanguageDetectorTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;

        public PhpLanguageDetectorTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRoot = testFixture.RootDirPath;
        }

        [Fact]
        public void Detect_ReturnsNull_WhenSourceDirectoryIsEmpty()
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(supportedPhpVersions: new[] { PhpVersions.Php73Version });
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
            var detector = CreatePhpLanguageDetector(supportedPhpVersions: new[] { PhpVersions.Php73Version });
            var repo = new MemorySourceRepo();
            repo.AddFile("foo.php content", "foo.php");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        public static TheoryData<string[], string, string> SupportedVersions
        {
            get
            {
                var data = new TheoryData<string[], string, string>();
                data.Add(new[] { "6.11.0" }, "6.11.0", "=6.11.0");
                data.Add(new[] { "4", "5", "8" }, "8.12.8", "=8.12.8");
                data.Add(new[] { ">=4 <13" }, "8.12.8", "=8.12.8");
                data.Add(new[] { ">=4 <13" }, "12.14.1", "=12.14.1");
                data.Add(new[] { ">=4 <13" }, ">=5 <9", ">=5.0.0 <9.0.0");
                data.Add(new[] { ">=4 <13" }, ">=5 <9 || 12.10", ">=5.0.0 <9.0.0 || >=12.10.0 <12.11.0");
                data.Add(new[] { ">=4 <13" }, ">=5 <9 || 14.10", ">=5.0.0 <9.0.0");
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(SupportedVersions))]
        public void Detect_ReturnsResult_ForSupportedVersions(
            string[] supportedVersions,
            string providedVersion,
            string expectedVersion)
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(supportedVersions);
            var repo = new MemorySourceRepo();
            repo.AddFile("{\"require\":{\"php\":\"" + providedVersion + "\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);
            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PhpName, result.Language);
            Assert.Equal(expectedVersion, result.LanguageVersion);
        }

        [Theory]
        [InlineData("4")]
        [InlineData("4.9")]
        [InlineData("8")]
        [InlineData("8.0.0")]
        public void Detect_Throws_WhenUnsupportedPhpVersion_FoundInComposerFile(string version)
        {
            // Arrange
            var supportedVersions = PhpVersionProvider.SupportedVersions;
            var detector = CreatePhpLanguageDetector(supportedVersions);
            var repo = new MemorySourceRepo();
            repo.AddFile("{\"require\":{\"php\":\"" + version + "\"}}", PhpConstants.ComposerFileName);
            var context = CreateContext(repo);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => detector.Detect(context));
            Assert.Contains(
                $"Platform 'php' version '{version}' is unsupported. " +
                $"Supported versions: {string.Join(",", supportedVersions)}",
                exception.Message);
        }

        [Theory]
        [InlineData("invalid json")]
        [InlineData("{\"data\": \"valid but meaningless\"}")]
        public void Detect_ReturnsResult_WithPhpDefaultRuntimeVersion_WithComposerFile(string composerFileContent)
        {
            // Arrange
            var detector = CreatePhpLanguageDetector(supportedPhpVersions: new[] { PhpVersions.Php73Version });
            var repo = new MemorySourceRepo();
            repo.AddFile(composerFileContent, PhpConstants.ComposerFileName);
            repo.AddFile("<?php echo true; ?>", "foo.php");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PhpConstants.PhpName, result.Language);
            Assert.Equal(PhpVersions.Php73Version, result.LanguageVersion);
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private PhpLanguageDetector CreatePhpLanguageDetector(string[] supportedPhpVersions)
        {
            return CreatePhpLanguageDetector(supportedPhpVersions, new TestEnvironment());
        }

        private PhpLanguageDetector CreatePhpLanguageDetector(string[] supportedPhpVersions, IEnvironment environment)
        {
            var optionsSetup = new PhpScriptGeneratorOptionsSetup(environment);
            var options = new PhpScriptGeneratorOptions();
            optionsSetup.Configure(options);

            return new PhpLanguageDetector(
                Options.Create(options),
                new TestPhpVersionProvider(supportedPhpVersions),
                NullLogger<PhpLanguageDetector>.Instance,
                new DefaultStandardOutputWriter());
        }

        private class TestPhpVersionProvider : IPhpVersionProvider
        {
            public TestPhpVersionProvider(string[] supportedPhpVersions)
            {
                SupportedPhpVersions = supportedPhpVersions;
            }

            public IEnumerable<string> SupportedPhpVersions { get; }
        }
    }
}
