// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class PrepareEnvironmentCommandProperty : CommandBaseProperty
    {
        public string SourceDir { get; set; }

        public bool SkipDetection { get; set; }

        public string PlatformsAndVersions { get; set; }

        public string PlatformsAndVersionsFile { get; set; }
    }
}
