// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DefaultScriptGeneratorProviderTest
    {
        [Fact]
        public void ReturnsScriptGenerator_WhichCanGenerateScript_WhenLanguageProvided_IsNull()
        {
            // Arrange
            var expected = new TestScriptGenerator("lang1", new[] { "1.0" }, canGenerateScript: true);
            var provider = CreateDefaultScriptGeneratorProvider(new[] { expected });
            var context = CreateScriptGeneratorContext(new TestSourceRepo(), languageName: null);

            // Act
            var actual = provider.GetScriptGenerator(context);

            // Assert
            Assert.Same(expected, actual);
        }

        [Fact]
        public void ReturnsScriptGenerator_WhichCanGenerateScript_WhenLanguageProvided_IsEmpty()
        {
            // Arrange
            var expected = new TestScriptGenerator("lang1", new[] { "1.0" }, canGenerateScript: true);
            var provider = CreateDefaultScriptGeneratorProvider(new[] { expected });
            var context = CreateScriptGeneratorContext(new TestSourceRepo(), languageName: null);

            // Act
            var actual = provider.GetScriptGenerator(context);

            // Assert
            Assert.Same(expected, actual);
        }

        [Fact]
        public void GetScriptGenerator_ReturnsNull_WhenNoLanguageIsProvided_AndCannotGenerateScript()
        {
            // Arrange
            var provider = CreateDefaultScriptGeneratorProvider(
                new[] { new TestScriptGenerator("lang1", new[] { "1.0" }, canGenerateScript: false) });
            var context = CreateScriptGeneratorContext(new TestSourceRepo(), languageName: null);

            // Act
            var scriptGenerator = provider.GetScriptGenerator(context);

            // Assert
            Assert.Null(scriptGenerator);
        }

        [Fact]
        public void GetScriptGenerator_ReturnsNull_ForUnsupportedLanguage()
        {
            // Arrange
            var provider = CreateDefaultScriptGeneratorProvider(
                new[] { new TestScriptGenerator("lang1", new[] { "1.0" }) });
            var context = CreateScriptGeneratorContext(new TestSourceRepo(), languageName: "unsuppotedLanguage");

            // Act
            var scriptGenerator = provider.GetScriptGenerator(context);

            // Assert
            Assert.Null(scriptGenerator);
        }

        [Fact]
        public void GetScriptGenerator_ReturnsNull_ForSupportedLanguage_ButUnsupportedVersion()
        {
            // Arrange
            var scriptGenerator = new TestScriptGenerator("lang1", new[] { "1.0.0" });
            var provider = CreateDefaultScriptGeneratorProvider(new[] { scriptGenerator });
            var context = CreateScriptGeneratorContext(
                new TestSourceRepo(),
                languageName: "lang1",
                languageVersion: "2.0.0");

            // Act
            var actual = provider.GetScriptGenerator(context);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetScriptGenerator_ReturnsFirstScriptGenerator_WhichSupportsLanguage()
        {
            // Arrange
            var expected = new TestScriptGenerator("lang1", new[] { "1.0" });
            var scriptGenerator2 = new TestScriptGenerator("lang1", new[] { "2.0" });
            var provider = CreateDefaultScriptGeneratorProvider(new[] { expected, scriptGenerator2 });
            var context = CreateScriptGeneratorContext(new TestSourceRepo(), languageName: "lang1");

            // Act
            var actual = provider.GetScriptGenerator(context);

            // Assert
            Assert.Same(expected, actual);
        }

        [Fact]
        public void GetScriptGenerator_ReturnsFirstScriptGenerator_WhichSupportsLanguageAndVersion()
        {
            // Arrange
            var scriptGenerator1 = new TestScriptGenerator("lang1", new[] { "1.0.0" });
            var expected = new TestScriptGenerator("lang1", new[] { "2.0.0" });
            var provider = CreateDefaultScriptGeneratorProvider(new[] { scriptGenerator1, expected });
            var context = CreateScriptGeneratorContext(
                new TestSourceRepo(),
                languageName: "lang1",
                languageVersion: "2.0.0");

            // Act
            var actual = provider.GetScriptGenerator(context);

            // Assert
            Assert.Same(expected, actual);
        }

        [Fact]
        public void GetScriptGenerator_ReturnsNull_IfScriptGeneratorCannotGenerateSript()
        {
            // Arrange
            var provider = CreateDefaultScriptGeneratorProvider(
                new[] { new TestScriptGenerator("lang1", new[] { "1.0" }, canGenerateScript: false) });
            var context = CreateScriptGeneratorContext(new TestSourceRepo(), languageName: "lang1");

            // Act
            var scriptGenerator = provider.GetScriptGenerator(context);

            // Assert
            Assert.Null(scriptGenerator);
        }

        [Fact]
        public void GetScriptGenerator_ReturnsScriptGenerator_IfScriptGeneratorCanGenerateSript()
        {
            // Arrange
            var expected = new TestScriptGenerator("lang1", new[] { "1.0" }, canGenerateScript: true);
            var provider = CreateDefaultScriptGeneratorProvider(new[] { expected });
            var context = CreateScriptGeneratorContext(new TestSourceRepo(), languageName: "lang1");

            // Act
            var scriptGenerator = provider.GetScriptGenerator(context);

            // Assert
            Assert.Same(expected, scriptGenerator);
        }

        [Fact]
        public void GetScriptGenerator_ReturnsScriptGenerator_IgnoresCaseOfLanguageName()
        {
            // Arrange
            var expected = new TestScriptGenerator("nodeJS", new[] { "1.0" }, canGenerateScript: true);
            var provider = CreateDefaultScriptGeneratorProvider(new[] { expected });
            var context = CreateScriptGeneratorContext(new TestSourceRepo(), languageName: "nodejs");

            // Act
            var scriptGenerator = provider.GetScriptGenerator(context);

            // Assert
            Assert.Same(expected, scriptGenerator);
        }

        [Fact]
        public void GetScriptGenerator_ReturnsScriptGenerator_IfOnlyMajorAndMinorVersionsAreProvided()
        {
            // Arrange
            var expected = new TestScriptGenerator("lang1", new[] { "1.2.3" });
            var provider = CreateDefaultScriptGeneratorProvider(new[] { expected });
            var context = CreateScriptGeneratorContext(
                new TestSourceRepo(),
                languageName: "lang1",
                languageVersion: "1.2");

            // Act
            var actual = provider.GetScriptGenerator(context);

            // Assert
            Assert.Same(expected, actual);
        }

        [Fact]
        public void GetScriptGenerator_ReturnsScriptGenerator_IfMajorMinorAndPatchVersionsAreProvided()
        {
            // Arrange
            var expected = new TestScriptGenerator("lang1", new[] { "1.2.3" });
            var provider = CreateDefaultScriptGeneratorProvider(new[] { expected });
            var context = CreateScriptGeneratorContext(
                new TestSourceRepo(),
                languageName: "lang1",
                languageVersion: "1.2.3");

            // Act
            var actual = provider.GetScriptGenerator(context);

            // Assert
            Assert.Same(expected, actual);
        }

        [Theory]
        [InlineData("1.2.3", "1.2.2")]
        [InlineData("1.3.2", "1.2.2")]
        [InlineData("1.3", "1.2.2")]
        [InlineData("2.2.2", "1.2.2")]
        public void GetScriptGenerator_ReturnsNull_IfLanguageVersionIsNotSupported(
            string providedLanguageVersion,
            string supportedVersions)
        {
            // Arrange
            var supportedLanguageVersions = supportedVersions.Split(',');
            var provider = CreateDefaultScriptGeneratorProvider(
                new[] { new TestScriptGenerator("lang1", supportedLanguageVersions, canGenerateScript: false) });
            var context = CreateScriptGeneratorContext(new TestSourceRepo(), "lang1", providedLanguageVersion);

            // Act
            var scriptGenerator = provider.GetScriptGenerator(context);

            // Assert
            Assert.Null(scriptGenerator);
        }

        private DefaultScriptGeneratorProvider CreateDefaultScriptGeneratorProvider(
            IEnumerable<IScriptGenerator> scriptGenerators)
        {
            return new DefaultScriptGeneratorProvider(
                scriptGenerators,
                NullLogger<DefaultScriptGeneratorProvider>.Instance);
        }

        private static ScriptGeneratorContext CreateScriptGeneratorContext(
            ISourceRepo sourceRepo,
            string languageName = null,
            string languageVersion = null)
        {
            return new ScriptGeneratorContext
            {
                LanguageName = languageName,
                LanguageVersion = languageVersion,
                SourceRepo = sourceRepo
            };
        }

        private class TestScriptGenerator : IScriptGenerator
        {
            private readonly bool? _canGenerateScript;

            public TestScriptGenerator(string languageName, string[] languageVersions)
                : this(languageName, languageVersions, canGenerateScript: null)
            {
            }

            public TestScriptGenerator(string languageName, string[] languageVersions, bool? canGenerateScript)
            {
                SupportedLanguageName = languageName;
                SupportedLanguageVersions = languageVersions;
                _canGenerateScript = canGenerateScript;
            }

            public string SupportedLanguageName { get; }

            public IEnumerable<string> SupportedLanguageVersions { get; }

            public bool CanGenerateScript(ScriptGeneratorContext context)
            {
                if (_canGenerateScript.HasValue)
                {
                    return _canGenerateScript.Value;
                }

                return true;
            }

            public string GenerateBashScript(ScriptGeneratorContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class TestSourceRepo : ISourceRepo
        {
            public string RootPath => throw new NotImplementedException();

            public bool FileExists(params string[] paths)
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
