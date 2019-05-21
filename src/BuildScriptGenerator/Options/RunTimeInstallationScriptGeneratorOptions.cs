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
        public string InstallationDir { get; set; }

        public string Platform { get; set; }

        public string PlatformVersion { get; set; }
    }
}
