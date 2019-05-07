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
        public void ReplaceUrlPasswords_Sanity()
        {
            Assert.Equal("https://bla:***@example.com/", "https://bla:blabla@example.com/".ReplaceUrlPasswords());
            Assert.Equal("https://bla:***@example.com/", "https://bla:123456@example.com/".ReplaceUrlPasswords());
            Assert.Equal("https://bla:***@example.com/", "https://bla:123bla@example.com/".ReplaceUrlPasswords());
            Assert.Equal("https://bla:*@example.com/", "https://bla:bla123@example.com/".ReplaceUrlPasswords("*"));

            Assert.Equal("https://bla:***@example.com/ and then ftp://user:***@example.org/subdir",
                "https://bla:123bla@example.com/ and then ftp://user:pass@example.org/subdir".ReplaceUrlPasswords());
        }
    }
}
