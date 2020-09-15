// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
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
            return GetInstallerScriptSnippet(RubyConstants.PlatformName, version);
        }

        public virtual bool IsVersionAlreadyInstalled(string version)
        {
            return IsVersionInstalled(
                version,
                builtInDir: RubyConstants.InstalledRubyVersionsDir,
                dynamicInstallDir: Path.Combine(CommonOptions.DynamicInstallRootDir, RubyConstants.PlatformName));
        }
    }
}
