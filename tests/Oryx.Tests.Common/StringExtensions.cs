// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.Tests.Common
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
