// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Used to declare another class as a build-time checker.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CheckerAttribute : Attribute
    {
        private readonly string[] targetToolNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckerAttribute"/> class.
        /// </summary>
        /// <param name="targetToolNames">
        /// The annotated checker will only be applied to builds that used these tools.
        /// If empty, the checker will apply to all builds, regardless of tools.
        /// </param>
        public CheckerAttribute(params string[] targetToolNames) => this.targetToolNames = targetToolNames;

        public string[] TargetToolNames => this.targetToolNames;
    }
}
