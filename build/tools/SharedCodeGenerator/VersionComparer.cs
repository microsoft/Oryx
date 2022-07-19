// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SharedCodeGenerator
{
    internal class VersionComparer : IComparer<string>
    {
        /// <summary>
        /// The Version class only supports the format of Major.Minor[.Patch][.Release] so this
        /// compares version strings that are either of the following formats:
        /// - Major.Minor[.Patch][.Release]
        /// - Major.Minor[.Patch][.Release]-<somestring>
        /// ------------------
        /// Example inputs:
        /// 6.0
        /// 5.9
        /// 6.0.1
        /// 6.0.100-preview.1.21103.13
        /// </summary>
        /// <returns>1 if s1 is greater, -1 if s2 is greater, 0 if the same</returns>
        public int Compare(string s1 = default, string s2 = default)
        {
            var split1 = s1.Split("-");
            var split2 = s2.Split("-");

            if (!Version.TryParse(split1[0], out var version1) || !Version.TryParse(split2[0], out var version2))
            {
                return s1.CompareTo(s2);
            }

            // if the beginning versions are the same, we need to dive deeper to
            // determine the correct order.
            // example: 6.0.100 and 6.0.100-rtm.21524.1
            if (version1.CompareTo(version2) == 0)
            {
                if (split1.Length != split2.Length)
                {
                    return split1.Length > split2.Length ? -1 : 1;
                }

                return s1.CompareTo(s2);
            }

            return version1.CompareTo(version2);
        }
    }
}
