// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// An abstraction of a segment of text, characterized by a beginning marker and an ending marker.
    /// </summary>
    internal class TextSpan : IEquatable<TextSpan>
    {
        public TextSpan(string name, string beginning, string ending)
        {
            this.Name = name;
            this.BeginMarker = beginning;
            this.EndMarker = ending;
        }

        public string Name { get; }

        public string BeginMarker { get; }

        public string EndMarker { get; }

        public override int GetHashCode() => this.Name.GetHashCode();

        public bool Equals(TextSpan that) => that != null && that.Name == this.Name;
    }
}
