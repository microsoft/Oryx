// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    public class PhpScriptGeneratorOptions
    {
        public string PhpVersion { get; set; }

        public string PhpDefaultVersion { get; set; }

        public string PhpComposerVersion { get; set; }

        public string PhpComposerDefaultVersion { get; set; }
    }
}