// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class NameAndValuePairParserTest
    {
        [Theory]
        [InlineData("showlog")]
        [InlineData("showlog=")]
        public void TryParse_ReturnsTrue_WhenOnlyKeyIsPresent(string pair)
        {
            // Arrange 
            var expectedKey = "showlog";
            var expectedValue = string.Empty;

            // Act
            var isValid = NameAndValuePairParser.TryParse(pair, out var key, out var value);

            // Assert
            Assert.True(isValid);
            Assert.Equal(expectedKey, key);
            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineData("a=bcd", "a", "bcd")]
        [InlineData("abc=d", "abc", "d")]
        [InlineData("ab=cd", "ab", "cd")]
        public void TryParse_ReturnsTrue_WhenBothKeyAndValueArePresent(
            string pair,
            string expectedKey,
            string expectedValue)
        {
            // Arrange & Act
            var isValid = NameAndValuePairParser.TryParse(pair, out var key, out var value);

            // Assert
            Assert.True(isValid);
            Assert.Equal(expectedKey, key);
            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineData("a=b=c=d", "a", "b=c=d")]
        [InlineData("a==", "a", "=")]
        [InlineData("a==b", "a", "=b")]
        public void MultipleEqualToSymbolsInPair_TryParse_ReturnsTrue_UsingFirstOccurrenceOfEqualToSymbol(
            string pair,
            string expectedKey,
            string expectedValue)
        {
            // Arrange & Act
            var isValid = NameAndValuePairParser.TryParse(pair, out var key, out var value);

            // Assert
            Assert.True(isValid);
            Assert.Equal(expectedKey, key);
            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void TryParse_ReturnsFalse_ForEmptyOrWhiteSpace(string pair)
        {
            // Arrange & Act
            var isValid = NameAndValuePairParser.TryParse(pair, out var key, out var value);

            // Assert
            Assert.False(isValid);
        }
    }
}
