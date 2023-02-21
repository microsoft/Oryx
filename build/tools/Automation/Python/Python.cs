using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Oryx.Automation.Client;
using Microsoft.Oryx.Automation.Models;
using Microsoft.Oryx.Automation.Python.Models;
using Microsoft.Oryx.Automation.Services;
using Microsoft.Oryx.Automation.Telemetry;
using Newtonsoft.Json;
using Oryx.Microsoft.Automation.Python;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.Oryx.Automation.Python
{
    public class Python
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger logger;
        private readonly IVersionService versionService;
        private readonly IYamlFileReaderService yamlFileReaderService;

        public Python(IHttpClient httpClient, ILogger logger, IVersionService versionService, IYamlFileReaderService yamlFileReaderService)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            this.versionService = versionService;
            this.yamlFileReaderService = yamlFileReaderService;
        }

        public async Task RunAsync(string oryxRootPath)
        {
            List<VersionObj> versionObjs = await this.GetNewVersionObjsAsync();
            if (versionObjs.Count > 0)
            {
                // Deserialize constants.yaml
                string constantsYamlAbsolutePath = Path.Combine(oryxRootPath, "build", Constants.ConstantsYaml);
                List<YamlConstants> yamlConstantsObjs = await this.yamlFileReaderService.ReadConstantsYamlFileAsync(constantsYamlAbsolutePath);

                this.UpdateOryxConstantsForNewVersions(versionObjs, yamlConstantsObjs, oryxRootPath);
            }
        }

        public async Task<List<VersionObj>> GetNewVersionObjsAsync()
        {
            var url = "https://www.python.org/api/v2/downloads/release/";
            var response = await this.httpClient.GetDataAsync(url);
            var releases = JsonConvert.DeserializeObject<List<Models.Release>>(response);

            url = "https://oryx-cdn.microsoft.io/python?restype=container&comp=list&include=metadata";
            HashSet<string> oryxSdkVersions = await this.httpClient.GetOryxSdkVersionsAsync(url);

            var versionObjs = new List<VersionObj>();
            foreach (var release in releases)
            {
                string newVersion = release.Name.Replace("Python", string.Empty).Trim();

                if (!release.PreRelease &&
                    this.versionService.IsVersionWithinRange(newVersion, minVersion: "3.10.9") &&
                    !oryxSdkVersions.Contains(newVersion))
                {
                    Console.WriteLine($"newVersion {newVersion}");
                    versionObjs.Add(new VersionObj
                    {
                        Version = newVersion,
                        GpgKey = this.GetGpgKeyForVersion(newVersion),
                    });
                }
            }

            return versionObjs;
        }

        private void UpdateOryxConstantsForNewVersions(List<VersionObj> versionObjs, List<YamlConstants> yamlConstants, string oryxRootPath)
        {
            Dictionary<string, YamlConstants> pythonYamlConstants = this.GetYamlPythonConstants(yamlConstants);

            foreach (var versionObj in versionObjs)
            {
                string version = versionObj.Version;
                string pythonConstantKey = this.GeneratePythonConstantKey(versionObj);
                Console.WriteLine($"[UpdateConstants] version: {version} pythonConstantKey: {pythonConstantKey}");
                pythonYamlConstants["python-versions"].Constants[pythonConstantKey] = version;

                this.UpdateVersionsToBuildTxt(versionObj, oryxRootPath);
            }

            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var constantsYamlAbsolutePath = Path.Combine(oryxRootPath, "build", Constants.ConstantsYaml);
            var stringResult = serializer.Serialize(yamlConstants);
            File.WriteAllText(constantsYamlAbsolutePath, stringResult);
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
            if (Constants.VersionGpgKeys.TryGetValue(majorMinor, out string gpgKey))
            {
                return gpgKey;
            }
            else
            {
                throw new ArgumentException($"GPG key not found for version {version}.");
            }
        }

        private string GeneratePythonConstantKey(VersionObj versionObj)
        {
            string[] splitVersion = versionObj.Version.Split('.');
            string majorVersion = splitVersion[0];
            string minorVersion = splitVersion[1];

            // prevent duplicate majorMinor where 11.0 and 1.10 both will generate a 110 key.
            int majorVersionInt = int.Parse(majorVersion);
            string majorMinor = majorVersionInt < 10 ? $"{majorVersion}{minorVersion}" : $"{majorVersion}Dot{minorVersion}";

            return $"python{majorMinor}-version";
        }

        private Dictionary<string, YamlConstants> GetYamlPythonConstants(List<YamlConstants> yamlContents)
        {
            var pythonConstants = yamlContents.Where(c => c.Name == Constants.YamlPythonKey)
                                  .ToDictionary(c => c.Name, c => c);
            return pythonConstants;
        }

        private void UpdateVersionsToBuildTxt(VersionObj platformConstant, string oryxRootPath)
        {
            HashSet<string> debianFlavors = new HashSet<string>() { "bullseye", "buster", "focal-scm", "stretch" };
            foreach (string debianFlavor in debianFlavors)
            {
                var versionsToBuildTxtAbsolutePath = Path.Combine(
                    oryxRootPath, "platforms", Constants.PythonName, "versions", debianFlavor, Constants.VersionsToBuildTxt);
                string line = $"\n{platformConstant.Version}, {platformConstant.GpgKey},";
                File.AppendAllText(versionsToBuildTxtAbsolutePath, line);

                // sort
                Console.WriteLine($"[UpdateVersionsToBuildTxt] Updating {versionsToBuildTxtAbsolutePath}...");
                var contents = File.ReadAllLines(versionsToBuildTxtAbsolutePath);
                Array.Sort(contents);
                File.WriteAllLines(versionsToBuildTxtAbsolutePath, contents.Distinct());
            }
        }
    }
}
