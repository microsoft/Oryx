// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class BuildScriptGeneratorOptions
    {
        public string SourceDir { get; set; }

        public string IntermediateDir { get; set; }

        public string DestinationDir { get; set; }

        public string PlatformName { get; set; }

        public string PlatformVersion { get; set; }

        public bool ScriptOnly { get; set; }

        public IDictionary<string, string> Properties { get; set; }

        public string ManifestDir { get; set; }
    }
}