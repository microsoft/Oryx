// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    /// <summary>
    /// Generates an installation script snippet to install a PHP SDK.
    /// </summary>
    internal class PhpPlatformInstaller : PlatformInstallerBase
    {
        public PhpPlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILoggerFactory loggerFactory)
            : base(commonOptions, loggerFactory)
        {
        }

        public virtual string GetInstallerScriptSnippet(string version)
        {
            return this.GetInstallerScriptSnippet(PhpConstants.PlatformName, version);
        }

        public virtual bool IsVersionAlreadyInstalled(string version)
        {
            return this.IsVersionInstalled(
                version,
                builtInDir: PhpConstants.InstalledPhpVersionsDir,
                dynamicInstallDir: Path.Combine(this.CommonOptions.DynamicInstallRootDir, PhpConstants.PlatformName));
        }

        public override void InstallPlatformSpecificSkeletonDependencies(StringBuilder stringBuilder)
        {
            // Do nothing for image used by builder
        }
    }
}
