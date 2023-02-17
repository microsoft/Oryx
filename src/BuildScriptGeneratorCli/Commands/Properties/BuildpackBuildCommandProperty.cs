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
    public class BuildpackBuildCommandProperty : BuildCommandProperty
    {
        public string LayersDir { get; set; }

        public string PlatformDir { get; set; }

        public string PlanPath { get; set; }
    }
}
