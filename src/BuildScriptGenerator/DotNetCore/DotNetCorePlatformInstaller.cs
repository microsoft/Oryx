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

        public virtual string GetInstallerScriptSnippet(string runtimeVersion, string globalJsonSdkVersion)
        {
            string sdkVersion = null;
            if (string.IsNullOrEmpty(globalJsonSdkVersion))
            {
                var versionMap = _versionProvider.GetSupportedVersions();
                sdkVersion = versionMap[runtimeVersion];
                _logger.LogDebug(
                    "Generating installation script for sdk version {sdkVersion} based on " +
                    "runtime version {runtimeVersion}",
                    sdkVersion,
                    runtimeVersion);
            }
            else
            {
                sdkVersion = globalJsonSdkVersion;
                _logger.LogDebug(
                    "Generating installation script for sdk version {sdkVersion} based on global.json file.",
                    sdkVersion);
            }

            var dirToInstall =
                $"{DotNetCoreConstants.DynamicDotNetCoreSdkVersionsInstallDir}/{sdkVersion}";
            var sentinelFileDir =
                $"{DotNetCoreConstants.DynamicDotNetCoreRuntimeVersionsInstallDir}/{runtimeVersion}";

            var sdkInstallerScript = GetInstallerScriptSnippet(
                DotNetCoreConstants.PlatformName,
                sdkVersion,
                dirToInstall);

            // Create the following structure so that 'benv' tool can understand it as it already does.
            var scriptBuilder = new StringBuilder();
            scriptBuilder
            .AppendLine(sdkInstallerScript)
            .AppendLine($"mkdir -p {DotNetCoreConstants.DynamicDotNetCoreRuntimeVersionsInstallDir}/{runtimeVersion}")
            .AppendLine($"echo '{sdkVersion}' > {DotNetCoreConstants.DynamicDotNetCoreRuntimeVersionsInstallDir}/{runtimeVersion}/sdkVersion.txt")
            // Write out a sentinel file to indicate downlaod and extraction was successful
            .AppendLine($"echo > {sentinelFileDir}/{SdkStorageConstants.SdkDownloadSentinelFileName}");
            return scriptBuilder.ToString();
        }

        public virtual bool IsVersionAlreadyInstalled(string runtimeVersion, string globalJsonSdkVersion)
        {
            if (string.IsNullOrEmpty(globalJsonSdkVersion))
            {
                return IsVersionInstalled(
                    runtimeVersion,
                    builtInDir: DotNetCoreConstants.DefaultDotNetCoreRuntimeVersionsInstallDir,
                    dynamicInstallDir: DotNetCoreConstants.DynamicDotNetCoreRuntimeVersionsInstallDir);
            }
            else
            {
                return IsVersionInstalled(
                    globalJsonSdkVersion,
                    builtInDir: DotNetCoreConstants.DefaultDotNetCoreSdkVersionsInstallDir,
                    dynamicInstallDir: DotNetCoreConstants.DynamicDotNetCoreSdkVersionsInstallDir);
            }
        }
    }
}
