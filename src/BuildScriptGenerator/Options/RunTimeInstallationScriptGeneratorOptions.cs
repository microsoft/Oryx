// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class RunTimeInstallationScriptGeneratorOptions
    {
        public string DestinationDir { get; set; }

        public string Language { get; set; }

        public string LanguageVersion { get; set; }
    }
}
