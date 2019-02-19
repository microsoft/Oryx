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
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower()).Replace("-", string.Empty);
        }
    }
}
