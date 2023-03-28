// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Php;

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

        public override void InstallPlatformSpecificSkeletonDependencies(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"echo 'Installing {DotNetCoreConstants.PlatformName} specific dependencies...'");

            // .NET Core dependencies (this is universal for all versions of .NET Core)
            stringBuilder.AppendAptGetInstallPackages(
                "libc6",
                "libgcc1",
                "libgssapi-krb5-2",
                "libstdc++6",
                "zlib1g",
                "libuuid1",
                "libunwind8");
            stringBuilder.AppendLine("if grep -q cli \"/opt/oryx/.imagetype\"; then");
            InstallPythonToolingAndLanguage(stringBuilder);
            stringBuilder.AppendLine("fi");
        }
    }
}
