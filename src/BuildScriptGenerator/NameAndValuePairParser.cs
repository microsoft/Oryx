// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class NameAndValuePairParser
    {
        public static bool TryParse(string pair, out string key, out string value)
        {
            if (pair == null)
            {
                throw new ArgumentNullException(nameof(pair));
            }

            key = null;
            value = null;

            if (string.IsNullOrWhiteSpace(pair))
            {
                return false;
            }

            // We only care about the first instance of '=' even if there are multiple
            // (for example, the value itself could have that symbol in it)
            var equalToSymbolIndex = pair.IndexOf('=');
            if (equalToSymbolIndex < 0)
            {
                // Example: -p showlog (in this case the user might not want to give a value)
                key = pair;
                value = string.Empty;
            }
            else if (equalToSymbolIndex == 0)
            {
                // ignore incorrect value
                return false;
            }
            else
            {
                // -p showlog=true
                key = pair.Substring(0, equalToSymbolIndex);
                value = pair.Substring(equalToSymbolIndex + 1);
            }

            return true;
        }
    }
}