// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Xunit;

namespace Microsoft.Extensions.Logging
{
    public class LoggerAiExtensionsTest
    {
        [Fact]
        public void SplitByLength_Sanity()
        {
            // Empty string
            Assert.Equal(0, LoggerAiExtensions.Chunkify("", 3).Count);

            // 1-length chunks
            Assert.Equal(new[] { "a", "b", "c" }, LoggerAiExtensions.Chunkify("abc", 1));

            // Chunks of equal lengths
            Assert.Equal(new[] { "abc", "abc" }, LoggerAiExtensions.Chunkify("abcabc", 3));

            // Chunks of different lengths
            Assert.Equal(new[] { "abc", "ab" }, LoggerAiExtensions.Chunkify("abcab", 3));
        }
    }
}
