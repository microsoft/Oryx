// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Oryx.Common.Extensions
{
    public static class StringExtensions
    {
        private const string UrlPattern = @"(https?|ftp|git|git+ssh|git+http|git+https|git+file):\/\/" +
                                                    @"(?<userinfo>[^\s/$.?#@]+)@[^\s/$.?#].[^\s]*";

        private const string UrlUserInfoReplacement = "***";

        public static bool EqualsIgnoreCase(this string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        public static string JoinKeyValuePairs(
            IDictionary<string, string> pairs,
            string pairSep = " ",
            char kvSep = '=')
        {
            return string.Join(pairSep, pairs.Select(pair => pair.Key + kvSep + pair.Value));
        }

        /// <summary>
        /// Replaces the userinfo subcomponent of URLs in a string with asterisks.
        /// </summary>
        /// <param name="str">string to replace. </param>
        /// <returns>str with authentication information in URLs replaced with asterisks. </returns>
        public static string ReplaceUrlUserInfo(this string str)
        {
            try
            {
                StringBuilder result = new StringBuilder();
                var matches = Regex.Matches(str, UrlPattern, RegexOptions.IgnoreCase);

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

        /// <summary>
        /// Splits a string to chunks of the given maximum length.
        /// </summary>
        /// <param name="str">string to split. </param>
        /// <param name="maxLength">maximum length of each chunk. </param>
        /// <returns>list of chunks. </returns>
        public static IList<string> Chunkify(this string str, int maxLength)
        {
            var result = new List<string>();
            for (int i = 0; i < str.Length; i += maxLength)
            {
                result.Add(str.Substring(i, Math.Min(maxLength, str.Length - i)));
            }

            return result;
        }

        /// <summary>
        /// Hash a string using SHA-256.
        /// </summary>
        /// <param name="str">The string to hash.</param>
        /// <returns>The SHA-256 hash of the given string.</returns>
        public static string Hash(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                var hash = sha.ComputeHash(bytes);
                var result = new StringBuilder();
                foreach (var x in bytes)
                {
                    result.AppendFormat("{0:x2}", x);
                }

                return result.ToString();
            }
        }
    }
}
