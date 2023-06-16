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
    public class BuildCommandBaseProperty : CommandBaseProperty
    {
        public string SourceDir { get; set; }

        public string Platform { get; set; }

        public string PlatformVersion { get; set; }

        public bool ShouldPackage { get; set; }

        public string OsRequirements { get; set; }

        public string AppType { get; set; }

        public string BuildCommandFile { get; set; }

        public bool CompressDestinationDir { get; set; }

        public string[] Property { get; set; }

        public string DynamicInstallRootDir { get; set; }
    }
}
