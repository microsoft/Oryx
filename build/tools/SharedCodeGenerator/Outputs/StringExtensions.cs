// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Globalization;

namespace Microsoft.Oryx.SharedCodeGenerator.Outputs
{
    internal static class StringExtensions
    {
        public static string Camelize(this string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower())
                .Replace(ConstantCollection.NameSeparator, string.Empty);
        }

        public static string WrapValueInQuotes(this string str)
        {
            // If the user already wrapped the value in quotes, then we just return back the string as it is,
            // else we use single quotes by default
            if (str.StartsWith("'") && str.EndsWith("'"))
            {
                return str;
            }

            if (str.StartsWith("\"") && str.EndsWith("\""))
            {
                return str;
            }

            return $"'{str}'";
        }
    }
}
