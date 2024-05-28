// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Ruby
{
    public class RubyScriptGeneratorOptions
    {
        public string RubyVersion { get; set; }

        public string DefaultVersion { get; set; }

        public string CustomBuildCommand { get; set; }
    }
}