// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class DotNetCorePlatformInstaller : PlatformInstallerBase
    {
        public DotNetCorePlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILoggerFactory loggerFactory)
            : base(commonOptions, loggerFactory)
        {
        }

        public virtual string GetInstallerScriptSnippet(string version)
        {
            return this.GetInstallerScriptSnippet(DotNetCoreConstants.PlatformName, version);
        }

        public virtual bool IsVersionAlreadyInstalled(string version)
        {
            return this.IsVersionInstalled(
                version,
                builtInDir: DotNetCoreConstants.DefaultDotNetCoreSdkVersionsInstallDir,
                dynamicInstallDir: Path.Combine(
                    this.CommonOptions.DynamicInstallRootDir, DotNetCoreConstants.PlatformName));
        }
    }
}
