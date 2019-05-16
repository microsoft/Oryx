// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class CheckerExtensions
    {
        public static IEnumerable<IChecker> WhereApplicable(this IEnumerable<IChecker> checkers, IDictionary<string, string> tools)
        {
            return checkers.Where(checker =>
            {
                var attr = (CheckerAttribute)Attribute.GetCustomAttribute(checker.GetType().Assembly, typeof(CheckerAttribute));
                return attr.TargetToolNames.Intersect(tools.Keys).Count() > 0;
            });
        }
    }
}
