// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public abstract class PlatformInstallerBase
    {
        protected readonly BuildScriptGeneratorOptions _commonOptions;
        protected readonly IEnvironment _environment;
        protected readonly HttpClient _httpClient;

        public PlatformInstallerBase(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IEnvironment environment,
            IHttpClientFactory httpClientFactory)
        {
            _commonOptions = commonOptions.Value;
            _environment = environment;

            _httpClient = httpClientFactory.CreateClient();
            // NOTE: Setting user agent is required to avoid receiving 403 Forbidden response.
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("oryx", "1.0"));
        }

        public bool IsVersionAlreadyInstalled(string version)
        {
            if (IsDeveloperEnvironment())
            {
                return IsVersionInstalledInDeveloperImage(version);
            }

            return IsVersionInstalledInBuildImage(version);
        }

        public string GetInstallerScriptSnippet(string version)
        {
            if (IsDeveloperEnvironment())
            {
                return GetInstallerScriptSnippetForDeveloperImage(version);
            }

            return GetInstallerScriptSnippetForBuildImage(version);
        }

        public virtual IEnumerable<string> GetAvailableVersionsInStorage()
        {
            if (IsDeveloperEnvironment())
            {
                return GetAvailableVersionsForDeveloperImage();
            }

            return GetAvailableVersionsForBuildImage();
        }

        public abstract string GetInstallerScriptSnippetForBuildImage(string version);

        public abstract bool IsVersionInstalledInBuildImage(string version);

        public abstract IEnumerable<string> GetAvailableVersionsForBuildImage();

        public abstract string GetInstallerScriptSnippetForDeveloperImage(string version);

        public abstract bool IsVersionInstalledInDeveloperImage(string version);

        public abstract IEnumerable<string> GetAvailableVersionsForDeveloperImage();

        protected string GetInstallerScriptSnippetForBuildImage(
            string platformName,
            string version,
            string directoryToInstall = null)
        {
            var sdkStorageBaseUrl = GetPlatformBinariesStorageBaseUrl();

            var versionDirInTemp = directoryToInstall;
            if (string.IsNullOrEmpty(versionDirInTemp))
            {
                versionDirInTemp = $"{Constants.TempInstallationDirRoot}/{platformName}/{version}";
            }

            var tarFile = $"{version}.tar.gz";
            var snippet = new StringBuilder();
            snippet
                .AppendLine()
                .AppendLine("echo")
                .AppendLine($"echo Downloading and installing {platformName} version '{version}'...")
                .AppendLine($"rm -rf {versionDirInTemp}")
                .AppendLine($"mkdir -p {versionDirInTemp}")
                .AppendLine($"cd {versionDirInTemp}")
                .AppendLine($"curl -D headers.txt -SL \"{sdkStorageBaseUrl}/{platformName}/{platformName}-{version}.tar.gz\" --output {tarFile} >/dev/null 2>&1")
                .AppendLine("headerName=\"x-ms-meta-checksum\"")
                .AppendLine("checksumHeader=$(cat headers.txt | grep $headerName: | tr -d '\r')")
                .AppendLine("rm -f headers.txt")
                .AppendLine("checksumValue=${checksumHeader#\"$headerName: \"}")
                .AppendLine("echo")
                .AppendLine("echo Verifying checksum...")
                .AppendLine($"echo \"$checksumValue {version}.tar.gz\" | sha512sum -c -")
                .AppendLine("echo")
                .AppendLine($"tar -xzf {tarFile} -C .")
                .AppendLine($"rm -f {tarFile}")
                .AppendLine();
            return snippet.ToString();
        }

        protected IEnumerable<string> GetAvailableVersionsInBuildImage(
            string platformName,
            string versionMetadataElementName)
        {
            var sdkStorageBaseUrl = GetPlatformBinariesStorageBaseUrl();
            var blobList = _httpClient
                .GetStringAsync($"{sdkStorageBaseUrl}/{platformName}?restype=container&comp=list&include=metadata")
                .Result;
            var xdoc = XDocument.Parse(blobList);
            var supportedVersions = new List<string>();
            foreach (var runtimeVersionElement in xdoc.XPathSelectElements(
                $"//Blobs/Blob/Metadata/{versionMetadataElementName}"))
            {
                supportedVersions.Add(runtimeVersionElement.Value);
            }

            return supportedVersions;
        }

        protected bool IsVersionInstalledInBuildImage(string version, string[] installationDirs)
        {
            foreach (var installationDir in installationDirs)
            {
                var versionsFromDisk = VersionProviderHelper.GetVersionsFromDirectory(installationDir);
                var range = new SemVer.Range(version);
                var maxSatisfyingVersion = range.MaxSatisfying(versionsFromDisk);
                if (!string.IsNullOrEmpty(maxSatisfyingVersion))
                {
                    return true;
                }
            }
            return false;
        }

        protected bool IsDeveloperEnvironment()
        {
            if (_commonOptions.Properties != null
                && _commonOptions.Properties.TryGetValue("developerenvironment", out var result)
                && bool.TryParse(result, out var isDeveloperImage))
            {
                return isDeveloperImage;
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
