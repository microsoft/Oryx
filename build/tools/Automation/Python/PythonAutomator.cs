// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Oryx.Automation.Commons;
using Microsoft.Oryx.Automation.Models;
using Microsoft.Oryx.Automation.Python.Models;
using Microsoft.Oryx.Automation.Services;
using Newtonsoft.Json;
using Oryx.Microsoft.Automation.Python;
using Version = System.Version;

namespace Microsoft.Oryx.Automation.Python
{
    public class PythonAutomator
    {
        private readonly IHttpService httpService;
        private readonly IVersionService versionService;
        private readonly IFileService fileService;
        private readonly IYamlFileService yamlFileService;
        private string pythonMinReleaseVersion;
        private string pythonMaxReleaseVersion;
        private List<string> pythonBlockedVersions = new List<string>();
        private HashSet<string> oryxPythonSdkVersions;

        public PythonAutomator(
            IHttpService httpService,
            IVersionService versionService,
            IFileService fileService,
            IYamlFileService yamlFileService)
        {
            this.httpService = httpService;
            this.versionService = versionService;
            this.fileService = fileService;
            this.yamlFileService = yamlFileService;
        }

        public async Task RunAsync()
        {
            string oryxSdkStorageBaseUrl = Environment.GetEnvironmentVariable(Constants.OryxSdkStorageBaseUrlEnvVar);
            string sdkVersionsUrl = SdkStorageHelper.GetSdkStorageUrl(oryxSdkStorageBaseUrl, PythonConstants.PythonSuffixUrl);
            this.oryxPythonSdkVersions = await this.httpService.GetOryxSdkVersionsAsync(sdkVersionsUrl);
            this.pythonMinReleaseVersion = Environment.GetEnvironmentVariable(PythonConstants.PythonMinReleaseVersionEnvVar);
            this.pythonMaxReleaseVersion = Environment.GetEnvironmentVariable(PythonConstants.PythonMaxReleaseVersionEnvVar);
            var blockedVersions = Environment.GetEnvironmentVariable(PythonConstants.PythonBlockedVersionsEnvVar);
            this.pythonBlockedVersions = SdkStorageHelper.ExtractBlockedVersions(blockedVersions);

            List<PythonVersion> newPythonVersions = await this.GetNewPythonVersionsAsync();
            if (newPythonVersions.Count > 0)
            {
                string constantsYamlSubPath = Path.Combine("build", Constants.ConstantsYaml);
                List<ConstantsYamlFile> yamlConstantsObjs =
                    await this.yamlFileService.ReadConstantsYamlFileAsync(constantsYamlSubPath);

                this.UpdateOryxConstantsForNewVersions(newPythonVersions, yamlConstantsObjs);
            }
        }

        /// <summary>
        /// Retrieves a list of new Python versions from the Python release website,
        /// and filters the list based on specified criteria.
        /// The resulting list is sorted by semantic version number.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.
        /// The task result contains a list of PythonVersion
        /// objects representing the new Python versions that meet the specified criteria.</returns>
        public async Task<List<PythonVersion>> GetNewPythonVersionsAsync()
        {
            var response = await this.httpService.GetDataAsync(PythonConstants.PythonReleaseUrl);
            var releases = JsonConvert.DeserializeObject<List<Release>>(response);

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
                    !this.oryxPythonSdkVersions.Contains(newVersion))
                {
                    pythonVersions.Add(new PythonVersion
                    {
                        Version = newVersion,
                        GpgKey = this.GetGpgKeyForVersion(newVersion),
                    });
                }
            }

            return pythonVersions.Order().ToList();
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
            Version v;
            if (!Version.TryParse(version, out v))
            {
                throw new ArgumentException("Invalid version format");
            }

            string majorMinor = $"{v.Major}.{v.Minor}";

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
            string majorMinor = majorVersionInt < 10 ? $"{majorVersion}{minorVersion}" : $"{majorVersion}_{minorVersion}";

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
