// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CheckerAttribute : Attribute
    {
        private readonly string[] _targetToolNames;

        public CheckerAttribute(params string[] targetToolNames) => _targetToolNames = targetToolNames;

        public string[] TargetToolNames => _targetToolNames;
    }
}
