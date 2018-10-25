// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Oryx.Tests.Infrastructure;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DefaultScriptGeneratorTest : IClassFixture<TestTempDirTestFixure>
    {
        private readonly string _tempDirRoot;

        public DefaultScriptGeneratorTest(TestTempDirTestFixure testFixure)
        {
            _tempDirRoot = testFixure.RootDirPath;
        }

        [Fact]
        public void TryGenerateScript_ReturnsTrue_IfNoLanguageIsProvided_AndCanDetectLanguage()
        {
            // Arrange
            var detector = new TestLanguageDetector("test", "1.0.0");
            var languageGenerator = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content");
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(new TestSourceRepo());

            // Act
            var canGenerateScript = generator.TryGenerateBashScript(context, out var generatedScript);

            // Assert  
            Assert.True(canGenerateScript);
            Assert.Equal("script-content", generatedScript);
        }

        [Fact]
        public void TryGenerateScript_Throws_IfNoLanguageIsProvided_AndCannotDetectLanguage()
        {
            // Arrange
            var detector = new TestLanguageDetector();
            var languageGenerator = new TestLanguageScriptGenerator("test", new[] { "1.0.0" });
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(new TestSourceRepo());

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => generator.TryGenerateBashScript(context, out var generatedScript));
            Assert.Equal("Could not detect the language from source directory.", exception.Message);
        }

        [Fact]
        public void TryGenerateScript_Throws_IfCanDetectLanguage_AndLanguageIsUnsupported()
        {
            // Arrange
            var detector = new TestLanguageDetector("unsupported", "1.0.0");
            var languageGenerator = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content");
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(new TestSourceRepo());

            // Act & Assert
            var exception = Assert.Throws<UnsupportedLanguageException>(
                () => generator.TryGenerateBashScript(context, out var generatedScript));
            Assert.Equal(
                "The supplied language 'unsupported' is not supported. Supported languages are: test",
                exception.Message);
        }

        [Fact]
        public void TryGenerateScript_Throws_IfCanDetectLanguage_AndLanguageVersionIsUnsupported()
        {
            // Arrange
            var detector = new TestLanguageDetector("test", "2.0.0"); // Unsupported version
            var languageGenerator = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content");
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(new TestSourceRepo());

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(
                () => generator.TryGenerateBashScript(context, out var generatedScript));
            Assert.Equal(
                "The supplied language version '2.0.0' is not supported. Supported versions are: 1.0.0",
                exception.Message);
        }

        [Fact]
        public void TryGenerateScript_Throws_IsSuppliedLanguageIsUnsupported()
        {
            // Arrange
            var detector = new TestLanguageDetector("test", "1.0.0");
            var languageGenerator = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content");
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(new TestSourceRepo(), languageName: "unsupported");

            // Act & Assert
            var exception = Assert.Throws<UnsupportedLanguageException>(
                () => generator.TryGenerateBashScript(context, out var generatedScript));
            Assert.Equal(
                "The supplied language 'unsupported' is not supported. Supported languages are: test",
                exception.Message);
        }

        [Fact]
        public void TryGenerateScript_Throws_IsSuppliedLanguageVersionIsUnsupported()
        {
            // Arrange
            var detector = new TestLanguageDetector("test", "1.0.0");
            var languageGenerator = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content");
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(
                new TestSourceRepo(),
                languageName: "test",
                languageVersion: "2.0.0");

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(
                () => generator.TryGenerateBashScript(context, out var generatedScript));
            Assert.Equal(
                "The supplied language version '2.0.0' is not supported. Supported versions are: 1.0.0",
                exception.Message);
        }

        [Fact]
        public void TryGenerateScript_ReturnsFalse_IfGeneratorTryGenerateScript_IsFalse()
        {
            // Arrange
            var detector = new TestLanguageDetector("test", "1.0.0");
            var languageGenerator = new TestLanguageScriptGenerator(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: false,
                scriptContent: null);
            var generator = CreateDefaultScriptGenerator(detector, languageGenerator);
            var context = CreateScriptGeneratorContext(new TestSourceRepo());

            // Act
            var canGenerateScript = generator.TryGenerateBashScript(context, out var generatedScript);

            // Assert  
            Assert.False(canGenerateScript);
            Assert.Null(generatedScript);
        }

        [Fact]
        public void UsesMaxSatisfyingVersion_WhenOnlyMajorVersion_OfLanguageIsSpecified()
        {
            // Arrange
            var detector = new TestLanguageDetector("test", "1.0.0");
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
                new TestSourceRepo(),
                languageName: "test",
                languageVersion: "1");

            // Act
            var canGenerateScript = generator.TryGenerateBashScript(context, out var generatedScript);

            // Assert
            Assert.True(canGenerateScript);
            Assert.Equal("1.5.5-content", generatedScript);
        }

        [Fact]
        public void UsesMaxSatisfyingVersion_WhenOnlyMajorAndMinorVersion_OfLanguageIsSpecified()
        {
            // Arrange
            var detector = new TestLanguageDetector("test", "1.0.0");
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
                new TestSourceRepo(),
                languageName: "test",
                languageVersion: "1.1");

            // Act
            var canGenerateScript = generator.TryGenerateBashScript(context, out var generatedScript);

            // Assert
            Assert.True(canGenerateScript);
            Assert.Equal("1.1.5-content", generatedScript);
        }

        [Fact]
        public void GeneratesScript_UsingTheFirstLanguageGenerator_WhichCanGenerateScript()
        {
            // Arrange
            var detector = new TestLanguageDetector();
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
                new TestSourceRepo(),
                languageName: "test",
                languageVersion: "1.0.0");

            // Act
            var canGenerateScript = generator.TryGenerateBashScript(context, out var generatedScript);

            // Assert
            Assert.True(canGenerateScript);
            Assert.Equal("script-content", generatedScript);
        }

        private string CreateNewDir()
        {
            return Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString("N"))).FullName;
        }

        private DefaultScriptGenerator CreateDefaultScriptGenerator(
            ILanguageDetector languageDetector,
            ILanguageScriptGenerator generator)
        {
            return new DefaultScriptGenerator(
                new[] { languageDetector },
                new[] { generator },
                NullLogger<DefaultScriptGenerator>.Instance);
        }

        private DefaultScriptGenerator CreateDefaultScriptGenerator(
            ILanguageDetector[] languageDetectors,
            ILanguageScriptGenerator[] generators)
        {
            return new DefaultScriptGenerator(
                languageDetectors,
                generators,
                NullLogger<DefaultScriptGenerator>.Instance);
        }

        private static ScriptGeneratorContext CreateScriptGeneratorContext(
            ISourceRepo sourceRepo,
            string languageName = null,
            string languageVersion = null)
        {
            return new ScriptGeneratorContext
            {
                Language = languageName,
                LanguageVersion = languageVersion,
                SourceRepo = sourceRepo
            };
        }

        private class TestLanguageDetector : ILanguageDetector
        {
            private readonly string _languageName;
            private readonly string _languageVersion;

            public TestLanguageDetector()
                : this(languageName: null, languageVersion: null)
            {
            }

            public TestLanguageDetector(string languageName, string languageVersion)
            {
                _languageName = languageName;
                _languageVersion = languageVersion;
            }

            public LanguageDetectorResult Detect(ISourceRepo sourceRepo)
            {
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
        }
    }
}
