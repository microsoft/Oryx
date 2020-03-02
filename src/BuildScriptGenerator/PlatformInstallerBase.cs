// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public abstract class PlatformInstallerBase
    {
        protected readonly BuildScriptGeneratorOptions _commonOptions;
        protected readonly IEnvironment _environment;

        public PlatformInstallerBase(IOptions<BuildScriptGeneratorOptions> commonOptions, IEnvironment environment)
        {
            _commonOptions = commonOptions.Value;
            _environment = environment;
        }

        public abstract string GetInstallerScriptSnippet(string version);

        public abstract bool IsVersionAlreadyInstalled(string version);

        protected string GetInstallerScriptSnippet(
            string platformName,
            string version,
            string directoryToInstall = null)
        {
            var sdkStorageBaseUrl = GetPlatformBinariesStorageBaseUrl();

            var versionDirInTemp = directoryToInstall;
            if (string.IsNullOrEmpty(versionDirInTemp))
            {
                versionDirInTemp = $"{Constants.TemporaryInstallationDirectoryRoot}/{platformName}/{version}";
            }

            var tarFile = $"{version}.tar.gz";
            var snippet = new StringBuilder();
            snippet
                .AppendLine()
                .AppendLine("PLATFORM_SETUP_START=$SECONDS")
                .AppendLine("echo")
                .AppendLine(
                $"echo Downloading {platformName} version '{version}' to {versionDirInTemp}...")
                .AppendLine($"rm -rf {versionDirInTemp}")
                .AppendLine($"mkdir -p {versionDirInTemp}")
                .AppendLine($"cd {versionDirInTemp}")
                .AppendLine("PLATFORM_BINARY_DOWNLOAD_START=$SECONDS")
                .AppendLine(
                $"curl -D headers.txt -SL \"{sdkStorageBaseUrl}/{platformName}/{platformName}-{version}.tar.gz\" " +
                $"--output {tarFile} >/dev/null 2>&1")
                .AppendLine("PLATFORM_BINARY_DOWNLOAD_ELAPSED_TIME=$(($SECONDS - $PLATFORM_BINARY_DOWNLOAD_START))")
                .AppendLine("echo \"Downloaded in $PLATFORM_BINARY_DOWNLOAD_ELAPSED_TIME sec(s).\"")
                // Search header name ignoring case
                .AppendLine("echo Verifying checksum...")
                .AppendLine("headerName=\"x-ms-meta-checksum\"")
                .AppendLine("checksumHeader=$(cat headers.txt | grep -i $headerName: | tr -d '\\r')")
                // Change header and value to lowercase
                .AppendLine("checksumHeader=$(echo $checksumHeader | tr '[A-Z]' '[a-z]')")
                .AppendLine("checksumValue=${checksumHeader#\"$headerName: \"}")
                .AppendLine("rm -f headers.txt")
                .AppendLine($"echo \"$checksumValue {version}.tar.gz\" | sha512sum -c - >/dev/null 2>&1")
                .AppendLine("echo Extracting contents...")
                .AppendLine($"tar -xzf {tarFile} -C .")
                .AppendLine($"rm -f {tarFile}")
                .AppendLine("PLATFORM_SETUP_ELAPSED_TIME=$(($SECONDS - $PLATFORM_SETUP_START))")
                .AppendLine("echo \"Done in $PLATFORM_SETUP_ELAPSED_TIME sec(s).\"")
                .AppendLine("echo");
            return snippet.ToString();
        }

        protected bool IsVersionInstalled(string lookupVersion, string[] installationDirs)
        {
            foreach (var installationDir in installationDirs)
            {
                var versionsFromDisk = VersionProviderHelper.GetVersionsFromDirectory(installationDir);
                if (versionsFromDisk.Any(onDiskVersion
                    => string.Equals(lookupVersion, onDiskVersion, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetPlatformBinariesStorageBaseUrl()
        {
            var platformBinariesStorageBaseUrl = _environment.GetEnvironmentVariable(
                SdkStorageConstants.SdkStorageBaseUrlKeyName);
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
