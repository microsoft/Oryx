// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class BuildPropertyAttribute : Attribute
    {
        public BuildPropertyAttribute(string name)
            : this(name, description: null)
        {
        }

        public BuildPropertyAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
