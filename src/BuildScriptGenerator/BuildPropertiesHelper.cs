// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class BuildPropertiesHelper
    {
        public static bool IsTrue(string propertyKeyName, BuildScriptGeneratorContext context, bool valueIsRequired)
        {
            if (context.Properties != null &&
                context.Properties.TryGetValue(propertyKeyName, out string value))
            {
                if (!valueIsRequired && string.IsNullOrWhiteSpace(value))
                {
                    return true;
                }

                if (string.Equals("true", value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
