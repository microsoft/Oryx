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
    /// Generates an installation script snippet to install PHP Composer.
    /// </summary>
    internal class PhpComposerInstaller : PlatformInstallerBase
    {
        public PhpComposerInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILoggerFactory loggerFactory)
            : base(commonOptions, loggerFactory)
        {
        }

        public virtual string GetInstallerScriptSnippet(string version)
        {
            return this.GetInstallerScriptSnippet(platformName: "php-composer", version);
        }

        public virtual bool IsVersionAlreadyInstalled(string version)
        {
            return this.IsVersionInstalled(
                version,
                builtInDir: PhpConstants.InstalledPhpComposerVersionDir,
                dynamicInstallDir: Path.Combine(this.CommonOptions.DynamicInstallRootDir, "php-composer"));
        }

        public override void InstallPlatformSpecificSkeletonDependencies(StringBuilder stringBuilder)
        {
            // Do nothing for image used by builder
        }
    }
}
