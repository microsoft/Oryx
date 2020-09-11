// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Java
{
    public class JavaPlatformInstaller : PlatformInstallerBase
    {
        public JavaPlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILoggerFactory loggerFactory)
            : base(commonOptions, loggerFactory)
        {
        }

        public virtual string GetInstallerScriptSnippet(string version)
        {
            return GetInstallerScriptSnippet(JavaConstants.PlatformName, version);
        }

        public virtual bool IsVersionAlreadyInstalled(string version)
        {
            return IsVersionInstalled(
                version,
                builtInDir: JavaConstants.InstalledJavaVersionsDir,
                dynamicInstallDir: Path.Combine(CommonOptions.DynamicInstallRootDir, JavaConstants.PlatformName));
        }
    }
}
