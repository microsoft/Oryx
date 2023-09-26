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
            this.CommonOptions = commonOptions.Value;
            this.Logger = loggerFactory.CreateLogger(this.GetType());
        }

        protected BuildScriptGeneratorOptions CommonOptions { get; }

        protected ILogger Logger { get; }

        /// <summary>
        /// Install tooling that is common to all platforms.
        /// </summary>
        public static void InstallCommonSkeletonDependencies(StringBuilder stringBuilder)
        {
            // Do nothing for builder image (packages are installed via APT buildpack)
        }

        public static void InstallPythonToolingAndLanguage(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("echo 'Installing python tooling and language...'");

            // Install Python tooling
            stringBuilder.AppendLine("PYTHONIOENCODING=\"UTF-8\"");
            stringBuilder.AppendAptGetInstallPackages(
                "make",
                "unzip",
                "libpq-dev",
                "moreutils",
                "python3-pip",
                "swig",
                "unixodbc-dev",
                "build-essential", // Adding additional python 3 packages to support all optional python modules: https://devguide.python.org/getting-started/setup-building/index.html#install-dependencies
                "gdb",
                "lcov",
                "pkg-config",
                "libbz2-dev",
                "libffi-dev",
                "libgdbm-dev",
                "liblzma-dev",
                "libncurses5-dev",
                "libreadline6-dev",
                "libsqlite3-dev",
                "libssl-dev",
                "lzma",
                "lzma-dev",
                "tk-dev",
                "uuid-dev",
                "zlib1g-dev");

            // Install Python 3.8
            stringBuilder.AppendLine("tmpDir=\"/opt/tmp\"");
            stringBuilder.AppendLine("imagesDir=\"$tmpDir/images\"");
            stringBuilder.AppendLine("buildDir=\"$tmpDir/build\"");
            stringBuilder.AppendLine("mkdir -p /usr/local/share/pip-cache/lib");
            stringBuilder.AppendLine("chmod -R 777 /usr/local/share/pip-cache");
            stringBuilder.AppendLine("pip3 install pip --upgrade");
            stringBuilder.AppendLine("python3 -m pip install --upgrade cython");
            stringBuilder.AppendLine("pip3 install --upgrade cython");
            stringBuilder.AppendLine(". $buildDir/__pythonVersions.sh");
            stringBuilder.AppendLine("$imagesDir/installPlatform.sh python $PYTHON38_VERSION");
            stringBuilder.AppendLine("[ -d \"/opt/python/$PYTHON38_VERSION\" ] && echo /opt/python/$PYTHON38_VERSION/lib >> /etc/ld.so.conf.d/python.conf");
            stringBuilder.AppendLine("ldconfig");
            stringBuilder.AppendLine("cd /opt/python");
            stringBuilder.AppendLine("ln -s $PYTHON38_VERSION 3.8");
            stringBuilder.AppendLine("ln -s $PYTHON38_VERSION latest");
            stringBuilder.AppendLine("ln -s $PYTHON38_VERSION stable");
        }

        public static void InstallGolangToolingAndLanguage(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("echo 'Installing golang tooling and language...'");
            stringBuilder.AppendLine("BUILD_DIR=\"/opt/tmp/build\"");
            stringBuilder.AppendLine("IMAGES_DIR=\"/opt/tmp/images\"");
            stringBuilder.AppendLine(". ${BUILD_DIR}/__goVersions.sh");
            stringBuilder.AppendLine("echo \"${GO_VERSION}\"");
            stringBuilder.AppendLine("downloadedFileName=\"go${GO_VERSION}.linux-amd64.tar.gz\"");
            stringBuilder.AppendLine("echo \"${downloadedFileName}\"");
            stringBuilder.AppendLine("${IMAGES_DIR}/retry.sh \"curl -SLsO https://golang.org/dl/$downloadedFileName\"");
            stringBuilder.AppendLine("mkdir -p /usr/local");
            stringBuilder.AppendLine("gzip -d $downloadedFileName");
            stringBuilder.AppendLine("tar -xf \"go${GO_VERSION}.linux-amd64.tar\" -C /usr/local");
            stringBuilder.AppendLine("rm -rf $downloadedFileName");
        }

        public virtual void InstallPlatformSpecificSkeletonDependencies(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("echo 'No platform specific dependencies to install.'");
        }

        protected string GetInstallerScriptSnippet(
            string platformName,
            string version,
            string directoryToInstall = null)
        {
            var sdkStorageBaseUrl = this.GetPlatformBinariesStorageBaseUrl();
            var sdkStorageAccountAccessToken = this.CommonOptions.OryxSdkStorageAccountAccessToken;

            var versionDirInTemp = directoryToInstall;
            if (string.IsNullOrEmpty(versionDirInTemp))
            {
                versionDirInTemp = Path.Combine(this.CommonOptions.DynamicInstallRootDir, platformName, version);
            }

            var tarFile = $"{version}.tar.gz";
            var snippet = new StringBuilder();
            snippet
                .AppendLine()
                .AppendLine("PLATFORM_SETUP_START=$SECONDS")
                .AppendLine("echo")
                .AppendLine(
                $"echo \"Downloading and extracting '{platformName}' version '{version}' to '{versionDirInTemp}'...\"")
                .AppendLine($"rm -rf {versionDirInTemp}")
                .AppendLine($"mkdir -p {versionDirInTemp}")
                .AppendLine($"cd {versionDirInTemp}")
                .AppendLine("PLATFORM_BINARY_DOWNLOAD_START=$SECONDS")
                .AppendLine($"platformName=\"{platformName}\"")
                .AppendLine($"export DEBIAN_FLAVOR={this.CommonOptions.DebianFlavor}")
                .AppendLine("echo \"Detected image debian flavor: $DEBIAN_FLAVOR.\"")
                .AppendLine($"if [ \"$DEBIAN_FLAVOR\" == \"{OsTypes.DebianStretch}\" ]; then")
                .AppendLine(
                $"curl -D headers.txt -SL \"{sdkStorageBaseUrl}/{platformName}/{platformName}-{version}.tar.gz$ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN\" " +
                $"--output {tarFile} >/dev/null 2>&1")
                .AppendLine("else")
                .AppendLine(
                $"curl -D headers.txt -SL \"{sdkStorageBaseUrl}/{platformName}/{platformName}-$DEBIAN_FLAVOR-{version}.tar.gz$ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN\" " +
                $"--output {tarFile} >/dev/null 2>&1")
                .AppendLine("fi")
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
                .AppendLine("echo Extracting contents...")
                .AppendLine($"tar -xzf {tarFile} -C .")

                // use sha256 for golang and sha512 for all other platforms
                .AppendLine($"if [ \"$platformName\" = \"golang\" ]; then")
                .AppendLine($"echo \"performing sha256sum for : {platformName}...\"")
                .AppendLine($"echo \"$checksumValue {version}.tar.gz\" | sha256sum -c - >/dev/null 2>&1")
                .AppendLine("else")
                .AppendLine($"echo \"performing sha512 checksum for: {platformName}...\"")
                .AppendLine($"echo \"$checksumValue {version}.tar.gz\" | sha512sum -c - >/dev/null 2>&1")
                .AppendLine("fi")
                .AppendLine($"rm -f {tarFile}")
                .AppendLine("PLATFORM_SETUP_ELAPSED_TIME=$(($SECONDS - $PLATFORM_SETUP_START))")
                .AppendLine("echo \"Done in $PLATFORM_SETUP_ELAPSED_TIME sec(s).\"")
                .AppendLine("echo")
                .AppendLine("oryxImageDetectorFile=\"/opt/oryx/.imagetype\"")
                .AppendLine("oryxOsDetectorFile=\"/opt/oryx/.ostype\"")

                // Write out a sentinel file to indicate download and extraction was successful
                .AppendLine($"echo > {Path.Combine(versionDirInTemp, SdkStorageConstants.SdkDownloadSentinelFileName)}");

            return snippet.ToString();
        }

        protected bool IsVersionInstalled(string lookupVersion, string builtInDir, string dynamicInstallDir)
        {
            var versionsFromDisk = VersionProviderHelper.GetVersionsFromDirectory(builtInDir);
            if (HasVersion(versionsFromDisk))
            {
                this.Logger.LogDebug(
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
                    this.Logger.LogDebug(
                        "Version {version} is already installed at directory {installationDir}",
                        lookupVersion,
                        dynamicInstallDir);

                    return true;
                }

                this.Logger.LogDebug(
                    "Directory for version {version} was already found at directory {installationDir}, " +
                    "but sentinel file {sentinelFile} was not found.",
                    lookupVersion,
                    dynamicInstallDir,
                    SdkStorageConstants.SdkDownloadSentinelFileName);
            }

            this.Logger.LogDebug(
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
            var platformBinariesStorageBaseUrl = this.CommonOptions.OryxSdkStorageBaseUrl;
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
