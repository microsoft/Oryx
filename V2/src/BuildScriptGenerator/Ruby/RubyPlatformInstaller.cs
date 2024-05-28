// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Ruby
{
    public class RubyPlatformInstaller : PlatformInstallerBase
    {
        public RubyPlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILoggerFactory loggerFactory)
            : base(commonOptions, loggerFactory)
        {
        }

        public virtual string GetInstallerScriptSnippet(string version)
        {
            return this.GetInstallerScriptSnippet(RubyConstants.PlatformName, version);
        }

        public virtual bool IsVersionAlreadyInstalled(string version)
        {
            return this.IsVersionInstalled(
                version,
                builtInDir: RubyConstants.InstalledRubyVersionsDir,
                dynamicInstallDir: Path.Combine(this.CommonOptions.DynamicInstallRootDir, RubyConstants.PlatformName));
        }

        public override void InstallPlatformSpecificSkeletonDependencies(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"echo 'Installing {RubyConstants.PlatformName} specific dependencies...'");

            stringBuilder.AppendAptGetInstallPackages(
                "libreadline-dev",
                "bzip2",
                "build-essential",
                "libssl-dev",
                "zlib1g-dev",
                "libpq-dev",
                "libsqlite3-dev",
                "patch",
                "gawk",
                "g++",
                "gcc",
                "make",
                "libc6-dev",
                "patch",
                "libreadline6-dev",
                "libyaml-dev",
                "sqlite3",
                "autoconf",
                "libgdbm-dev",
                "libncurses5-dev",
                "automake",
                "libtool",
                "bison",
                "pkg-config",
                "libffi-dev",
                "bison",
                "libxslt-dev",
                "libxml2-dev",
                "wget",
                "git",
                "net-tools",
                "dnsutils",
                "curl",
                "tcpdump",
                "iproute2",
                "unixodbc-dev",
                "vim",
                "tcptraceroute");
        }
    }
}
