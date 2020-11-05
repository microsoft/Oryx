// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public abstract class PlatformInstallerBase
    {
        public PlatformInstallerBase(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILoggerFactory loggerFactory)
        {
            CommonOptions = commonOptions.Value;
            Logger = loggerFactory.CreateLogger(GetType());
        }

        protected BuildScriptGeneratorOptions CommonOptions { get; }

        protected ILogger Logger { get; }

        protected string GetInstallerScriptSnippet(
            string platformName,
            string version,
            string directoryToInstall = null)
        {
            var sdkStorageBaseUrl = GetPlatformBinariesStorageBaseUrl();

            if (string.IsNullOrEmpty(directoryToInstall))
            {
                directoryToInstall = CommonOptions.DynamicInstallRootDir;
            }

            return $"/opt/oryx/installPlatform " +
                $"--platform {platformName} " +
                $"--platform-version {version} " +
                $"--base-target-dir {directoryToInstall} " +
                $"--base-download-url {sdkStorageBaseUrl}";
        }

        protected bool IsVersionInstalled(string lookupVersion, string builtInDir, string dynamicInstallDir)
        {
            var versionsFromDisk = VersionProviderHelper.GetVersionsFromDirectory(builtInDir);
            if (HasVersion(versionsFromDisk))
            {
                Logger.LogDebug(
                    "Version {version} is already installed at directory {installationDir}",
                    lookupVersion,
                    builtInDir);

                return true;
            }

            versionsFromDisk = VersionProviderHelper.GetVersionsFromDirectory(dynamicInstallDir);
            if (HasVersion(versionsFromDisk))
            {
                // Only if there is a sentinel file we want to indicate that a version exists.
                // This is because a user could kill a build midway which might leave the download of an SDK
                // in a corrupt state.
                var sentinelFile = Path.Combine(
                    dynamicInstallDir,
                    lookupVersion,
                    SdkStorageConstants.SdkDownloadSentinelFileName);

                if (File.Exists(sentinelFile))
                {
                    Logger.LogDebug(
                        "Version {version} is already installed at directory {installationDir}",
                        lookupVersion,
                        dynamicInstallDir);

                    return true;
                }

                Logger.LogDebug(
                    "Directory for version {version} was already found at directory {installationDir}, " +
                    "but sentinel file {sentinelFile} was not found.",
                    lookupVersion,
                    dynamicInstallDir,
                    SdkStorageConstants.SdkDownloadSentinelFileName);
            }

            Logger.LogDebug(
                "Version {version} was not found to be installed at {builtInDir} or {dynamicInstallDir}",
                lookupVersion,
                builtInDir,
                dynamicInstallDir);

            return false;

            bool HasVersion(IEnumerable<string> versionsOnDisk)
            {
                return versionsOnDisk.Any(onDiskVersion
                    => string.Equals(lookupVersion, onDiskVersion, StringComparison.OrdinalIgnoreCase));
            }
        }

        private string GetPlatformBinariesStorageBaseUrl()
        {
            var platformBinariesStorageBaseUrl = CommonOptions.OryxSdkStorageBaseUrl;
            if (string.IsNullOrEmpty(platformBinariesStorageBaseUrl))
            {
                throw new InvalidOperationException(
                    $"Environment variable '{SdkStorageConstants.SdkStorageBaseUrlKeyName}' is required.");
            }

            platformBinariesStorageBaseUrl = platformBinariesStorageBaseUrl.TrimEnd('/');
            return platformBinariesStorageBaseUrl;
        }
    }
}
