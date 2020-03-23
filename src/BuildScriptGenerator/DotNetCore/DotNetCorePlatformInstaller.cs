// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class DotNetCorePlatformInstaller : PlatformInstallerBase
    {
        private readonly IDotNetCoreVersionProvider _versionProvider;

        public DotNetCorePlatformInstaller(
            IOptions<BuildScriptGeneratorOptions> cliOptions,
            IDotNetCoreVersionProvider versionProvider,
            ILoggerFactory loggerFactory)
            : base(cliOptions, loggerFactory)
        {
            _versionProvider = versionProvider;
        }

        public override string GetInstallerScriptSnippet(string runtimeVersion)
        {
            var versionMap = _versionProvider.GetSupportedVersions();
            var sdkVersion = versionMap[runtimeVersion];
            var dirToInstall =
                $"{Constants.TemporaryInstallationDirectoryRoot}/{DotNetCoreConstants.PlatformName}/sdks/{sdkVersion}";
            var sentinelFileDir =
                $"{Constants.TemporaryInstallationDirectoryRoot}/{DotNetCoreConstants.PlatformName}/runtimes/{runtimeVersion}";
            var sdkInstallerScript = GetInstallerScriptSnippet(
                DotNetCoreConstants.PlatformName,
                sdkVersion,
                dirToInstall);
            var dotnetDir = $"{Constants.TemporaryInstallationDirectoryRoot}/{DotNetCoreConstants.PlatformName}";

            // Create the following structure so that 'benv' tool can understand it as it already does.
            var scriptBuilder = new StringBuilder();
            scriptBuilder
            .AppendLine(sdkInstallerScript)
            .AppendLine($"mkdir -p {dotnetDir}/runtimes/{runtimeVersion}")
            .AppendLine($"echo '{sdkVersion}' > {dotnetDir}/runtimes/{runtimeVersion}/sdkVersion.txt")
            // Write out a sentinel file to indicate downlaod and extraction was successful
            .AppendLine($"echo > {sentinelFileDir}/{SdkStorageConstants.SdkDownloadSentinelFileName}");
            return scriptBuilder.ToString();
        }

        public override bool IsVersionAlreadyInstalled(string version)
        {
            return IsVersionInstalled(
                version,
                builtInDir: DotNetCoreConstants.InstalledDotNetCoreRuntimeVersionsDir,
                dynamicInstallDir: $"{Constants.TemporaryInstallationDirectoryRoot}/dotnet/runtimes");
        }
    }
}
