// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Detector.Exceptions;
using Microsoft.Oryx.Detector.Resources;
using Tomlyn.Model;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests
{
    public class ParserHelperTest
    {
        [Fact]
        public void ThrowsGeneralParserExceptionWhenFailingToParseInvalidTomlFile()
        {
            // Arrange
            var fileName = "test.toml";
            var expectedMessage = string.Format(Messages.FailedToParseFileExceptionFormat, fileName);
            var repo = new MemorySourceRepo();
            repo.AddFile("invaid text", fileName);

            // Act & Assert
            var exception = Assert.Throws<FailedToParseFileException>(
                () => ParserHelper.ParseTomlFile(repo, fileName));
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Equal(fileName, exception.FilePath);
        }

        [Fact]
        public void ThrowsGeneralParserExceptionWhenFailingToParseInvalidYamlFile()
        {
            // Arrange
            var fileName = "test.yaml";
            var expectedMessage = string.Format(Messages.FailedToParseFileExceptionFormat, fileName);
            var repo = new MemorySourceRepo();
            repo.AddFile("\"test", fileName);

            // Act & Assert
            var exception = Assert.Throws<FailedToParseFileException>(
                () => ParserHelper.ParseYamlFile(repo, fileName));
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Equal(fileName, exception.FilePath);
        }

        [Fact]
        public void DoesNotThrowFailedToParseException_IfTomlFileHasLessStrictOrdering()
        {
            var lessStrictTomlFileContent = 
                @"baseURL = 'http://example.org/'
                languageCode = 'en-us'
                title = 'My New Hugo Site'
                theme = 'ananke'

                [params.plugins]
                    URL = 'plugins/bootstrap/bootstrap.min.css'

                [params]
                home = 'Home'
            ";
            // Arrange
            var fileName = "test.toml";
            var repo = new MemorySourceRepo();
            repo.AddFile(lessStrictTomlFileContent, fileName);

            // Act
            var result = ParserHelper.ParseTomlFile(repo, fileName);
            var urlContent = ((TomlTable)((TomlTable) result["params"])["plugins"])["URL"];
            var homeConent = ((TomlTable) result["params"])["home"];

            // Assert
            Assert.NotNull(result);
            Assert.Equal("plugins/bootstrap/bootstrap.min.css", urlContent);
            Assert.Equal("Home", homeConent);
        }
    }
}
