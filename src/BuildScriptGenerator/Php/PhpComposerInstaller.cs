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
            _ = stringBuilder.AppendLine($"echo 'Installing php-composer specific dependencies...'");

            // Install an assortment of traditional tooling (unicode, SSL, HTTP, etc.)
            _ = stringBuilder.AppendLine("if [ \"${DEBIAN_FLAVOR}\" = \"buster\" ]; then");
            _ = stringBuilder.AppendAptGetInstallPackages(
                "ca-certificates",
                "libargon2-0",
                "libcurl4-openssl-dev",
                "libedit-dev",
                "libonig-dev",
                "libncurses6",
                "libsodium-dev",
                "libsqlite3-dev",
                "libxml2-dev",
                "xz-utils");
            _ = stringBuilder.AppendLine("else");
            _ = stringBuilder.AppendLine("tmpDir=\"/opt/tmp\"");
            _ = stringBuilder.AppendLine("imagesDir=\"$tmpDir/images\"");
            _ = stringBuilder.AppendLine("$imagesDir/build/php/prereqs/installPrereqs.sh");
            _ = stringBuilder.AppendAptGetInstallPackages(
                "libcurl3",
                "libicu57",
                "liblttng-ust0",
                "libssl1.0.2",
                "libargon2-0",
                "libonig-dev",
                "libncurses5-dev",
                "libxml2-dev",
                "libedit-dev");
            _ = stringBuilder.AppendLine("fi");
        }
    }
}
