// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DefaultScriptGeneratorTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;

        public DefaultScriptGeneratorTest(TestTempDirTestFixture testFixure)
        {
            _tempDirRoot = testFixure.RootDirPath;
        }

        [Fact]
        public void TryGenerateScript_ReturnsTrue_IfNoLanguageIsProvided_AndCanDetectLanguage()
        {
            // Arrange
            var detector = new TestLanguageDetector(
                detectedLanguageName: "test",
                detectedLanguageVersion: "1.0.0");
            var languageGenerator = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content");
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(
                suppliedLanguageName: null,
                suppliedLanguageVersion: null);

            // Act
            var canGenerateScript = generator.TryGenerateBashScript(context, out var generatedScript);

            // Assert
            Assert.True(canGenerateScript);
            Assert.Equal("script-content", generatedScript);
            Assert.True(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_ReturnsTrue_IfLanguageIsProvidedButNoVersion_AndCanDetectVersion()
        {
            // Arrange
            var detector = new TestLanguageDetector(
                detectedLanguageName: "test",
                detectedLanguageVersion: "1.0.0");
            var languageGenerator = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content");
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext();
            context.Language = "test";
            context.LanguageVersion = null; // version not provided by user

            // Act
            var canGenerateScript = generator.TryGenerateBashScript(context, out var generatedScript);

            // Assert
            Assert.Equal("1.0.0", context.LanguageVersion);
            Assert.Equal("script-content", generatedScript);
            Assert.True(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_Throws_IfNoLanguageIsProvided_AndCannotDetectLanguage()
        {
            // Arrange
            var detector = new TestLanguageDetector(
                detectedLanguageName: null,
                detectedLanguageVersion: null);
            var languageGenerator = new TestLanguageScriptGenerator("test", new[] { "1.0.0" });
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(
                suppliedLanguageName: null,
                suppliedLanguageVersion: null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => generator.TryGenerateBashScript(context, out var generatedScript));
            Assert.Equal("Could not detect the language and/or version from repo", exception.Message);
            Assert.True(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_Throws_IfLanguageIsProvidedButNoVersion_AndCannotDetectVersion()
        {
            // Arrange
            var detector = new TestLanguageDetector(
                detectedLanguageName: "test",
                detectedLanguageVersion: null);
            var languageGenerator = new TestLanguageScriptGenerator("test", new[] { "1.0.0" });
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(
                suppliedLanguageName: "test",
                suppliedLanguageVersion: null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => generator.TryGenerateBashScript(context, out var generatedScript));
            Assert.Equal("Could not detect the language and/or version from repo", exception.Message);
            Assert.True(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_Throws_IfCanDetectLanguage_AndLanguageIsUnsupported()
        {
            // Arrange
            var detector = new TestLanguageDetector(
                detectedLanguageName: "unsupported", // unsupported language
                detectedLanguageVersion: "1.0.0");
            var languageGenerator = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content");
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(
                suppliedLanguageName: null,
                suppliedLanguageVersion: null);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedLanguageException>(
                () => generator.TryGenerateBashScript(context, out var generatedScript));
            Assert.Equal(
                "'unsupported' language is not supported. Supported languages are: test",
                exception.Message);
            Assert.True(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_Throws_IfCanDetectLanguageVersion_AndLanguageVersionIsUnsupported()
        {
            // Arrange
            var detector = new TestLanguageDetector(
                detectedLanguageName: "test",
                detectedLanguageVersion: "2.0.0"); // Unsupported version
            var languageGenerator = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content");
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(
                suppliedLanguageName: null,
                suppliedLanguageVersion: null);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(
                () => generator.TryGenerateBashScript(context, out var generatedScript));
            Assert.Equal(
                "The 'test' version '2.0.0' is not supported. Supported versions are: 1.0.0",
                exception.Message);
            Assert.True(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_Throws_IfSuppliedLanguageIsUnsupported()
        {
            // Arrange
            var detector = new TestLanguageDetector(
                detectedLanguageName: "test",
                detectedLanguageVersion: "1.0.0");
            var languageGenerator = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content");
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(
                suppliedLanguageName: "unsupported",
                suppliedLanguageVersion: "1.0.0");

            // Act & Assert
            var exception = Assert.Throws<UnsupportedLanguageException>(
                () => generator.TryGenerateBashScript(context, out var generatedScript));
            Assert.Equal(
                "'unsupported' language is not supported. Supported languages are: test",
                exception.Message);
            Assert.False(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_Throws_IfSuppliedLanguageVersionIsUnsupported()
        {
            // Arrange
            var detector = new TestLanguageDetector(
                detectedLanguageName: "test",
                detectedLanguageVersion: "1.0.0");
            var languageGenerator = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content");
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(
                suppliedLanguageName: "test",
                suppliedLanguageVersion: "2.0.0"); //unsupported version

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(
                () => generator.TryGenerateBashScript(context, out var generatedScript));
            Assert.Equal(
                "The 'test' version '2.0.0' is not supported. Supported versions are: 1.0.0",
                exception.Message);
            Assert.False(detector.DetectInvoked);
        }

        [Fact]
        public void TryGenerateScript_ReturnsFalse_IfGeneratorTryGenerateScript_IsFalse()
        {
            // Arrange
            var detector = new TestLanguageDetector(
                detectedLanguageName: "test",
                detectedLanguageVersion: "1.0.0");
            var languageGenerator = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: false,
                scriptContent: null);
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(
                suppliedLanguageName: null,
                suppliedLanguageVersion: null);

            // Act
            var canGenerateScript = generator.TryGenerateBashScript(context, out var generatedScript);

            // Assert  
            Assert.True(detector.DetectInvoked);
            Assert.False(canGenerateScript);
            Assert.Null(generatedScript);
        }

        [Fact]
        public void UsesMaxSatisfyingVersion_WhenOnlyMajorVersion_OfLanguageIsSpecified()
        {
            // Arrange
            var detector = new TestLanguageDetector(
                detectedLanguageName: "test",
                detectedLanguageVersion: "1.0.0");
            var languageGenerator1 = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.1.0" },
                canGenerateScript: true,
                scriptContent: "1.0.0-content");
            var languageGenerator2 = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.5.5" },
                canGenerateScript: true,
                scriptContent: "1.5.5-content");
            var generator = CreateDefaultScriptGenerator(
                new[] { detector },
                new[] { languageGenerator1, languageGenerator2 });
            var context = CreateScriptGeneratorContext(
                suppliedLanguageName: "test",
                suppliedLanguageVersion: "1");

            // Act
            var canGenerateScript = generator.TryGenerateBashScript(context, out var generatedScript);

            // Assert
            Assert.True(canGenerateScript);
            Assert.Equal("1.5.5-content", generatedScript);
            Assert.False(detector.DetectInvoked);
        }

        [Fact]
        public void UsesMaxSatisfyingVersion_WhenOnlyMajorAndMinorVersion_OfLanguageIsSpecified()
        {
            // Arrange
            var detector = new TestLanguageDetector(
                detectedLanguageName: "test",
                detectedLanguageVersion: "1.0.0");
            var languageGenerator1 = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.1.0" },
                canGenerateScript: true,
                scriptContent: "1.0.0-content");
            var languageGenerator2 = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.1.5" },
                canGenerateScript: true,
                scriptContent: "1.1.5-content");
            var generator = CreateDefaultScriptGenerator(
                new[] { detector },
                new[] { languageGenerator1, languageGenerator2 });
            var context = CreateScriptGeneratorContext(
                suppliedLanguageName: "test",
                suppliedLanguageVersion: "1.1");

            // Act
            var canGenerateScript = generator.TryGenerateBashScript(context, out var generatedScript);

            // Assert
            Assert.True(canGenerateScript);
            Assert.Equal("1.1.5-content", generatedScript);
            Assert.False(detector.DetectInvoked);
        }

        [Fact]
        public void GeneratesScript_UsingTheFirstLanguageGenerator_WhichCanGenerateScript()
        {
            // Arrange
            var detector = new TestLanguageDetector(
                detectedLanguageName: null,
                detectedLanguageVersion: null);
            var languageGenerator1 = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: false,
                scriptContent: null);
            var languageGenerator2 = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content");
            var generator = CreateDefaultScriptGenerator(
                new[] { detector },
                new[] { languageGenerator1, languageGenerator2 });
            var context = CreateScriptGeneratorContext(
                suppliedLanguageName: "test",
                suppliedLanguageVersion: "1.0.0");

            // Act
            var canGenerateScript = generator.TryGenerateBashScript(context, out var generatedScript);

            // Assert
            Assert.True(canGenerateScript);
            Assert.Equal("script-content", generatedScript);
            Assert.False(detector.DetectInvoked);
        }

        private string CreateNewDir()
        {
            return Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString("N"))).FullName;
        }

        private DefaultScriptGenerator CreateDefaultScriptGenerator(
            ILanguageDetector languageDetector,
            ILanguageScriptGenerator generator)
        {
            return new DefaultScriptGenerator(new[] { languageDetector }, new[] { generator }, NullLogger<DefaultScriptGenerator>.Instance);
        }

        private DefaultScriptGenerator CreateDefaultScriptGenerator(
            ILanguageDetector[] languageDetectors,
            ILanguageScriptGenerator[] generators)
        {
            return new DefaultScriptGenerator(languageDetectors, generators, NullLogger<DefaultScriptGenerator>.Instance);
        }

        private static ScriptGeneratorContext CreateScriptGeneratorContext(
            string suppliedLanguageName = null,
            string suppliedLanguageVersion = null)
        {
            return new ScriptGeneratorContext
            {
                Language = suppliedLanguageName,
                LanguageVersion = suppliedLanguageVersion,
                SourceRepo = new TestSourceRepo(),
            };
        }

        private class TestLanguageDetector : ILanguageDetector
        {
            private readonly string _languageName;
            private readonly string _languageVersion;

            public TestLanguageDetector(string detectedLanguageName, string detectedLanguageVersion)
            {
                _languageName = detectedLanguageName;
                _languageVersion = detectedLanguageVersion;
            }

            public bool DetectInvoked { get; private set; }

            public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
            {
                DetectInvoked = true;

                if (!string.IsNullOrEmpty(_languageName))
                {
                    return new LanguageDetectorResult
                    {
                        Language = _languageName,
                        LanguageVersion = _languageVersion,
                    };
                }
                return null;
            }
        }

        private class TestLanguageScriptGenerator : ILanguageScriptGenerator
        {
            private readonly bool? _canGenerateScript;
            private readonly string _scriptContent;

            public TestLanguageScriptGenerator(string languageName, string[] languageVersions)
                : this(languageName, languageVersions, canGenerateScript: null, scriptContent: null)
            {
            }

            public TestLanguageScriptGenerator(
                string languageName,
                string[] languageVersions,
                bool? canGenerateScript,
                string scriptContent)
            {
                SupportedLanguageName = languageName;
                SupportedLanguageVersions = languageVersions;
                _canGenerateScript = canGenerateScript;
                _scriptContent = scriptContent;
            }

            public string SupportedLanguageName { get; }

            public IEnumerable<string> SupportedLanguageVersions { get; }

            public bool TryGenerateBashScript(ScriptGeneratorContext scriptGeneratorContext, out string script)
            {
                script = null;

                if (_canGenerateScript.HasValue)
                {
                    if (_canGenerateScript.Value)
                    {
                        script = _scriptContent;
                    }

                    return _canGenerateScript.Value;
                }

                return false;
            }
        }

        private class TestSourceRepo : ISourceRepo
        {
            public string RootPath => throw new NotImplementedException();

            public bool FileExists(params string[] paths)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<string> EnumerateFiles(string searchPattern, bool searchSubDirectories)
            {
                throw new NotImplementedException();
            }

            public string ReadFile(params string[] paths)
            {
                throw new NotImplementedException();
            }

            public string[] ReadAllLines(params string[] paths)
            {
                throw new NotImplementedException();
            }

            public string GetGitCommitId() => null;
        }
    }
}
