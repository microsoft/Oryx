// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class CheckerExtensions
    {
        public static IEnumerable<IChecker> WhereApplicable(
            this IEnumerable<IChecker> checkers,
            IDictionary<string, string> tools)
        {
            return checkers.Where(checker =>
            {
                var attr = checker.GetType().GetCustomAttributes(typeof(CheckerAttribute), false)
                                            .FirstOrDefault() as CheckerAttribute;

                // If the checker wasn't annotated with the designated attribute, it shouldn't be used at all
                if (attr == null)
                {
                    return false;
                }

                // If the checker didn't specify a scope, it's global
                if (attr.TargetToolNames == null || attr.TargetToolNames.Length == 0)
                {
                    return true;
                }

                return attr.TargetToolNames.Intersect(tools.Keys).Any();
            });
        }
    }
}
