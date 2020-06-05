// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Oryx.Detector
{
    public static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
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
