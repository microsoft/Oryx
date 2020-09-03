// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Detector.Exceptions;
using Microsoft.Oryx.Detector.Resources;
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
    }
}
