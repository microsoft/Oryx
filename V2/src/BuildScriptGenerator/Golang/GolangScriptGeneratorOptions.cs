// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Golang
{
    public class GolangScriptGeneratorOptions
    {
        public string GolangVersion { get; set; }

        public string DefaultVersion { get; set; }

        public string CustomBuildCommand { get; set; }
    }
}