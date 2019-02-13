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
        private readonly string _type;

        public OutputTypeAttribute(string type)
        {
            _type = type;
        }

        public string Type => _type;
    }
}
