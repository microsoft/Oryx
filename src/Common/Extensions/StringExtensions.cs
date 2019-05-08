// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Oryx.Common.Extensions
{
    public static class StringExtensions
    {
        private static readonly string UrlPattern = @"(https?|ftp|git|git+ssh|git+http|git+https|git+file):\/\/" +
                                                    @"(?<userinfo>[^\s/$.?#@]+)@[^\s/$.?#].[^\s]*";

        private static readonly string UrlUserInfoReplacement = "***";

        /// <summary>
        /// Replaces the userinfo subcomponent of URLs in a string with asterisks.
        /// </summary>
        /// <param name="str">string to replace</param>
        /// <returns>str with authentication information in URLs replaced with asterisks</returns>
        public static string ReplaceUrlUserInfo(this string str)
        {
            try
            {
                StringBuilder result = new StringBuilder();
                ICollection<Match> matches = Regex.Matches(str, UrlPattern, RegexOptions.IgnoreCase);

                int positionInStr = 0;
                foreach (Match m in matches)
                {
                    var uig = m.Groups["userinfo"];
                    result.Append(str.Substring(positionInStr, uig.Index - positionInStr));
                    result.Append(UrlUserInfoReplacement);
                    positionInStr = uig.Index + uig.Length; // Skip past password
                }

                result.Append(str.Substring(positionInStr));
                return result.ToString();
            }
            catch
            {
                return str;
            }
        }
    }
}
