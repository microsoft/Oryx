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
                "before https://bla:123bla@example.com/ and then ftp://user:pass@example.org/subdir".ReplaceUrlUserInfo());
        }
    }
}
