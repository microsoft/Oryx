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

        public virtual string GetInstallerScriptSnippet(string version, bool skipSdkBinaryDownload = false)
        {
            return this.GetInstallerScriptSnippet(platformName: "php-composer", version, skipSdkBinaryDownload: skipSdkBinaryDownload);
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
            stringBuilder.AppendLine($"echo 'Installing php-composer specific dependencies...'");
            stringBuilder.AppendLine("if [[ \"${DEBIAN_FLAVOR}\" = \"buster\" || \"${DEBIAN_FLAVOR}\" = \"bullseye\" ]]; then");

            // Install an assortment of traditional tooling (unicode, SSL, HTTP, etc.)
            stringBuilder.AppendAptGetInstallPackages(
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
            stringBuilder.AppendLine("else");
            stringBuilder.AppendLine("tmpDir=\"/opt/tmp\"");
            stringBuilder.AppendLine("imagesDir=\"$tmpDir/images\"");
            stringBuilder.AppendLine("$imagesDir/build/php/prereqs/installPrereqs.sh");
            stringBuilder.AppendAptGetInstallPackages(
                "libcurl3",
                "libicu57",
                "liblttng-ust0",
                "libssl1.0.2",
                "libargon2-0",
                "libonig-dev",
                "libncurses5-dev",
                "libxml2-dev",
                "libedit-dev");
            stringBuilder.AppendLine("fi");
        }
    }
}
