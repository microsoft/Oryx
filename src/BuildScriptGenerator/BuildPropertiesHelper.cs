// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class BuildPropertiesHelper
    {
        public static bool IsTrue(string propertyKeyName, BuildScriptGeneratorContext context, bool valueIsRequired)
        {
            if (context.Properties != null &&
                context.Properties.TryGetValue(propertyKeyName, out string val))
            {
                if (!valueIsRequired && string.IsNullOrWhiteSpace(val))
                {
                    return true;
                }

                if (val.EqualsIgnoreCase(Constants.True))
                {
                    return true;
                }
            }

            return false;
        }
    }
}