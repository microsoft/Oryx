// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal abstract class BuildCommandBase : CommandBase
    {
        public string SourceDir { get; set; }

        public string PlatformName { get; set; }

        public string PlatformVersion { get; set; }

        public bool ShouldPackage { get; set; }

        public string OsRequirements { get; set; }

        public string AppType { get; set; }

        public string BuildCommandsFileName { get; set; }

        public bool CompressDestinationDir { get; set; }

        public string[] Properties { get; set; }

        public string DynamicInstallRootDir { get; set; }
    }
}
