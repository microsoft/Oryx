// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public class PythonPlatformInstaller : PlatformInstallerBase
    {
        public PythonPlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILoggerFactory loggerFactory)
            : base(commonOptions, loggerFactory)
        {
        }

        public virtual string GetInstallerScriptSnippet(string version)
        {
            return this.GetInstallerScriptSnippet(PythonConstants.PlatformName, version);
        }

        public virtual bool IsVersionAlreadyInstalled(string version)
        {
            return this.IsVersionInstalled(
                version,
                builtInDir: PythonConstants.InstalledPythonVersionsDir,
                dynamicInstallDir: Path.Combine(this.CommonOptions.DynamicInstallRootDir, PythonConstants.PlatformName));
        }

        public override void InstallPlatformSpecificSkeletonDependencies(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"echo 'Installing {PythonConstants.PlatformName} specific dependencies...'");
            stringBuilder.AppendLine("if grep -q cli \"/opt/oryx/.imagetype\"; then");
            InstallPythonToolingAndLanguage(stringBuilder);
            stringBuilder.AppendLine("fi");
        }
    }
}
