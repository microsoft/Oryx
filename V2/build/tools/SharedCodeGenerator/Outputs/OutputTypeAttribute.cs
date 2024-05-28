// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.SharedCodeGenerator.Outputs
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class OutputTypeAttribute : Attribute
    {
        public OutputTypeAttribute(string type)
        {
            this.Type = type;
        }

        public string Type { get; }
    }
}
