// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;

namespace Oryx.Tests.Infrastructure
{
    public static class StringExtensions
    {
        public static string ReplaceNewLine(this string original, string replacingString = "")
        {
            return original?
                .Replace(Environment.NewLine, replacingString)
                .Replace("\0", replacingString)
                .Replace("\r", replacingString);
        }
    }
}
