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

        public CheckerAttribute(params string[] toolNames) => _targetToolNames = toolNames;

        public string[] TargetToolNames => _targetToolNames;
    }
}
