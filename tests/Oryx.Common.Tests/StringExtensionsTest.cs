// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Xunit;

namespace Microsoft.Oryx.Common.Extensions
{
    public class StringExtensionsTest
    {
        [Fact]
        public void EqualsIgnoreCase_Sanity()
        {
            Assert.True("abc".EqualsIgnoreCase("aBc"));

            Assert.True("abc".EqualsIgnoreCase("ABC"));

            Assert.False("bl".EqualsIgnoreCase("bla"));

            string s = null;
            Assert.False(s.EqualsIgnoreCase("bla"));
            Assert.False("bla".EqualsIgnoreCase(s));
        }

        [Fact]
        public void Chunkify_Sanity()
        {
            // Empty string
            Assert.Equal(0, "".Chunkify(3).Count);

            // 1 chunk
            Assert.Equal(new[] { "abc" }, "abc".Chunkify(3));

            // 1-length chunks
            Assert.Equal(new[] { "a", "b", "c" }, "abc".Chunkify(1));

            // Chunks of equal lengths
            Assert.Equal(new[] { "abc", "abc" }, "abcabc".Chunkify(3));

            // Chunks of different lengths
            Assert.Equal(new[] { "abc", "ab" }, "abcab".Chunkify(3));
        }

        [Fact]
        public void ReplaceUrlUserInfo_Sanity()
        {
            Assert.Equal("http://***@example.com/", "http://bla:blabla@example.com/".ReplaceUrlUserInfo());

            Assert.Equal("https://***@example.com/", "https://bla:123456@example.com/".ReplaceUrlUserInfo());

            Assert.Equal("ftp://***@example.com/", "ftp://bla:123bla@example.com/".ReplaceUrlUserInfo());

            Assert.Equal("git://***@example.com/", "git://bla:bla123@example.com/".ReplaceUrlUserInfo());

            Assert.Equal(
                "just before https://***@sub.example.net",
                "just before https://root:to!1or@sub.example.net".ReplaceUrlUserInfo());

            Assert.Equal(
                "https://***@example.com/ only after",
                "https://bla:blabla@example.com/ only after".ReplaceUrlUserInfo());

            Assert.Equal(
                "before https://***@example.com/ and then ftp://***@example.org/subdir",
                "before https://bla:123bla@example.com/ and then ftp://user:pass@example.org/subdir"
                .ReplaceUrlUserInfo());
        }
    }
}
