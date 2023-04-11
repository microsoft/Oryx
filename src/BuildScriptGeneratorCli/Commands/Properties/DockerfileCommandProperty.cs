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
    public class DockerfileCommandProperty : CommandBaseProperty
    {
        public string SourceDir { get; set; }

        public string BuildImage { get; set; }

        public string Platform { get; set; }

        public string PlatformVersion { get; set; }

        public string RuntimePlatform { get; set; }

        public string RuntimePlatformVersion { get; set; }

        public string BindPort { get; set; }

        public string Output { get; set; }
    }
}
