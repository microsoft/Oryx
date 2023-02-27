// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Oryx.Automation.Extensions;
using Microsoft.Oryx.Automation.Models;
using Microsoft.Oryx.Automation.Python.Models;
using Microsoft.Oryx.Automation.Services;
using Newtonsoft.Json;
using Oryx.Microsoft.Automation.Python;

namespace Microsoft.Oryx.Automation.Python
{
    public class PythonAutomator : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly IVersionService versionService;
        private readonly IFileService fileService;
        private readonly IYamlFileService yamlFileService;
        private string pythonMinReleaseVersion;
        private string pythonMaxReleaseVersion;
        private List<string> pythonBlockedVersions;

        public PythonAutomator(
            IHttpClientFactory httpClientFactory,
            IVersionService versionService,
            IFileService fileService,
            IYamlFileService yamlFileService)
        {
            this.httpClient = httpClientFactory.CreateClient();
            this.versionService = versionService;
            this.fileService = fileService;
            this.yamlFileService = yamlFileService;
        }

        public async Task RunAsync()
        {
            List<PythonVersion> pythonVersions = await this.GetNewPythonVersionsAsync();
            if (pythonVersions.Count > 0)
            {
                this.pythonMinReleaseVersion = Environment.GetEnvironmentVariable(PythonConstants.PythonMinReleaseVersionEnvVar);
                this.pythonMaxReleaseVersion = Environment.GetEnvironmentVariable(PythonConstants.PythonMaxReleaseVersionEnvVar);
                var blockedVersions = Environment.GetEnvironmentVariable(
                    PythonConstants.PythonBlockedVersionsEnvVar);
                if (!string.IsNullOrEmpty(blockedVersions))
                {
                    var versionStrings = blockedVersions.Split(',');
                    foreach (var versionString in versionStrings)
                    {
                        this.pythonBlockedVersions.Add(versionString.Trim());
                    }
                }

                string constantsYamlSubPath = Path.Combine("build", Constants.ConstantsYaml);
                List<ConstantsYamlFile> yamlConstantsObjs =
                    await this.yamlFileService.ReadConstantsYamlFileAsync(constantsYamlSubPath);

                this.UpdateOryxConstantsForNewVersions(pythonVersions, yamlConstantsObjs);
            }
        }

        public async Task<List<PythonVersion>> GetNewPythonVersionsAsync()
        {
            var response = await this.httpClient.GetDataAsync(PythonConstants.PythonReleaseUrl);
            var releases = JsonConvert.DeserializeObject<List<Models.Release>>(response);

            HashSet<string> oryxSdkVersions = await this.httpClient.GetOryxSdkVersionsAsync(
                Constants.OryxSdkStorageBaseUrl + PythonConstants.PythonSuffixUrl);

            var pythonVersions = new List<PythonVersion>();
            foreach (var release in releases)
            {
                string newVersion = release.Name.Replace("Python", string.Empty).Trim();

                if (!release.PreRelease &&
                    this.versionService.IsVersionWithinRange(
                        newVersion,
                        minVersion: this.pythonMinReleaseVersion,
                        maxVersion: this.pythonMaxReleaseVersion,
                        this.pythonBlockedVersions) &&
                    !oryxSdkVersions.Contains(newVersion))
                {
                    pythonVersions.Add(new PythonVersion
                    {
                        Version = newVersion,
                        GpgKey = this.GetGpgKeyForVersion(newVersion),
                    });
                }
            }

            return pythonVersions;
        }

        public void Dispose()
        {
            this.httpClient.Dispose();
        }

        private void UpdateOryxConstantsForNewVersions(
            List<PythonVersion> pythonVersions, List<ConstantsYamlFile> constantsYamlFile)
        {
            Dictionary<string, ConstantsYamlFile> pythonYamlConstants = this.GetYamlPythonConstants(constantsYamlFile);

            foreach (var pythonVersion in pythonVersions)
            {
                string version = pythonVersion.Version;
                string pythonConstantKey = this.GeneratePythonConstantKey(pythonVersion);
                Console.WriteLine($"[UpdateConstants] version: {version} pythonConstantKey: {pythonConstantKey}");

                ConstantsYamlFile pythonYamlConstant = pythonYamlConstants[PythonConstants.ConstantsYamlPythonKey];
                pythonYamlConstant.Constants[pythonConstantKey] = version;

                // update versionsToBuild.txt
                string line = $"\n{pythonVersion.Version}, {pythonVersion.GpgKey},";
                this.fileService.UpdateVersionsToBuildTxt(PythonConstants.PythonName, line);
            }

            var constantsYamlSubPath = Path.Combine("build", Constants.ConstantsYaml);
            this.yamlFileService.WriteConstantsYamlFile(constantsYamlSubPath, constantsYamlFile);
        }

        private string GetGpgKeyForVersion(string version)
        {
            // Split the version string into major and minor parts
            string[] parts = version.Split('.');
            if (parts.Length < 2)
            {
                throw new ArgumentException("Invalid version format");
            }

            string majorMinor = string.Join(".", parts.Take(2));

            // Look up the GPG key in the dictionary
            if (PythonConstants.VersionGpgKeys.TryGetValue(majorMinor, out string gpgKey))
            {
                return gpgKey;
            }
            else
            {
                throw new ArgumentException($"GPG key not found for version {version}.");
            }
        }

        private string GeneratePythonConstantKey(PythonVersion pythonVersion)
        {
            string[] splitVersion = pythonVersion.Version.Split('.');
            string majorVersion = splitVersion[0];
            string minorVersion = splitVersion[1];

            // prevent duplicate majorMinor where 11.0 and 1.10 both will generate a 110 key.
            int majorVersionInt = int.Parse(majorVersion);
            string majorMinor = majorVersionInt < 10 ? $"{majorVersion}{minorVersion}" : $"{majorVersion}Dot{minorVersion}";

            return $"python{majorMinor}-version";
        }

        private Dictionary<string, ConstantsYamlFile> GetYamlPythonConstants(List<ConstantsYamlFile> yamlContents)
        {
            var pythonConstants = yamlContents.Where(c => c.Name == PythonConstants.ConstantsYamlPythonKey)
                                  .ToDictionary(c => c.Name, c => c);
            return pythonConstants;
        }
    }
}
