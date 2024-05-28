// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Golang
{
    public class GolangPlatformInstaller : PlatformInstallerBase
    {
        public GolangPlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILoggerFactory loggerFactory)
            : base(commonOptions, loggerFactory)
        {
        }

        public virtual string GetInstallerScriptSnippet(string version)
        {
            return this.GetInstallerScriptSnippet(GolangConstants.PlatformName, version);
        }

        public virtual bool IsVersionAlreadyInstalled(string version)
        {
            return this.IsVersionInstalled(
                version,
                builtInDir: GolangConstants.InstalledGolangVersionsDir,
                dynamicInstallDir: Path.Combine(this.CommonOptions.DynamicInstallRootDir, GolangConstants.PlatformName));
        }
    }
}
