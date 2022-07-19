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
