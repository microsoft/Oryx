// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public abstract class BuildCommandBaseBinder<T> : CommandBaseBinder<T>
        where T : BuildCommandBaseProperty
    {
        public BuildCommandBaseBinder(
            Argument<string> sourceDir,
            Option<string> platform,
            Option<string> platformVersion,
            Option<bool> shouldPackage,
            Option<string> osRequirements,
            Option<string> appType,
            Option<string> buildCommandFile,
            Option<bool> compressDestinationDir,
            Option<string[]> property,
            Option<string> dynamicInstallRootDir,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.SourceDir = sourceDir;
            this.Platform = platform;
            this.PlatformVersion = platformVersion;
            this.ShouldPackage = shouldPackage;
            this.OsRequirements = osRequirements;
            this.AppType = appType;
            this.BuildCommandFile = buildCommandFile;
            this.CompressDestinationDir = compressDestinationDir;
            this.Property = property;
            this.DynamicInstallRootDir = dynamicInstallRootDir;
        }

        protected Argument<string> SourceDir { get; set; }

        protected Option<string> Platform { get; set; }

        protected Option<string> PlatformVersion { get; set; }

        protected Option<bool> ShouldPackage { get; set; }

        protected Option<string> OsRequirements { get; set; }

        protected Option<string> AppType { get; set; }

        protected Option<string> BuildCommandFile { get; set; }

        protected Option<bool> CompressDestinationDir { get; set; }

        protected Option<string[]> Property { get; set; }

        protected Option<string> DynamicInstallRootDir { get; set; }
    }
}
