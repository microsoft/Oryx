// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class RunScriptGeneratorOptions
    {
        public ISourceRepo SourceRepo { get; set; }

        public string SourcePath { get; set; }

        public string UserStartupCommand { get; set; }

        public string DefaultAppPath { get; set; }
    }
}