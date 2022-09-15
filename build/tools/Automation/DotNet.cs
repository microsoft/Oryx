using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.Oryx.Automation
{
    /// <Summary>
    /// TODO: write summary.
    /// </Summary>
    public class DotNet : Program
    {
        /// <inheritdoc/>
        public override async Task<List<PlatformConstant>> GetVersionShaAsync()
        {
            List<PlatformConstant> platformConstants = new List<PlatformConstant>();

            // query https://github.com/dotnet/sdk/releases
            string releasesUrl = "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json";
            var response = await Request.RequestAsync(releasesUrl);

            // validate response
            if (response == null)
            {
                return platformConstants;
            }

            var json = JsonConvert.DeserializeObject<ReleaseNotes>(response);
            if (json == null || json.ReleasesIndex == null || json.ReleasesIndex[0] == null)
            {
                Console.WriteLine("empty");
                return platformConstants;
            }

            var versionAndShas = new Dictionary<string, string>();
            var releasesIndex = json.ReleasesIndex;
            foreach (var releaseIndex in releasesIndex)
            {
                // if released today add to dictionary
                var dateReleased = releaseIndex.LatestReleaseDate;
                if (ReleasedToday(dateReleased))
                {
                    // Console.WriteLine($"Released today: {dateReleased}");
                    releasesUrl = releaseIndex.ReleasesJson;
                    // Console.WriteLine($"releasesUrl: {releasesUrl}");
                    response = await Request.RequestAsync(releasesUrl);
                    if (response == null)
                    {
                        return platformConstants;
                    }

                    var releasesJson = JsonConvert.DeserializeObject<ReleasesJson>(response);
                    if (releasesJson == null)
                    {
                        return platformConstants;
                    }

                    var releases = releasesJson.Releases;
                    foreach (var release in releases)
                    {
                        if (!ReleasedToday(release.ReleaseDate)) continue;
                        Console.WriteLine($"release-date: {release.ReleaseDate}");

                        // sdk
                        string sdkVersion = release.Sdk.Version;
                        string sha = GetSha(release.Sdk.Files);
                        PlatformConstant platformConstant = new PlatformConstant(
                            sdkVersion, sha, "dotnet", "sdk");
                        platformConstants.Add(platformConstant);

                        // runtime (netcore)
                        string runtimeVersion = release.Runtime.Version;
                        sha = GetSha(release.Runtime.Files);
                        Console.WriteLine($"For Runtime: {runtimeVersion} {release.Runtime.VersionDisplay} {sha}");
                        platformConstant = new PlatformConstant(
                            runtimeVersion, sha, "dotnet", "net-core");
                        platformConstants.Add(platformConstant);

                        // runtime (aspnetcore)
                        string aspnetCoreRuntimeVersion = release.AspnetCoreRuntime.Version;
                        sha = GetSha(release.AspnetCoreRuntime.Files);
                        Console.WriteLine($"For AspnetCoreRuntime: {aspnetCoreRuntimeVersion} {release.AspnetCoreRuntime.VersionDisplay} {sha}");
                        platformConstant = new PlatformConstant(
                            runtimeVersion, sha, "dotnet", "aspnet-core");
                        platformConstants.Add(platformConstant);
                    }
                }
            }

            // query https://github.com/dotnet/core/blob/main/release-notes/7.0/releases.json#L56
            return platformConstants;
        }

        /// <inheritdoc/>
        public override async Task UpdateConstantsAsync(List<PlatformConstant> platformConstants)
        {
            // read constants.yaml
            string file = "build/constants.yaml";
            string fileContents = await File.ReadAllTextAsync(file);
            Console.WriteLine(fileContents);
            // deserialize
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            var yamlContents = deserializer.Deserialize<List<Constant>>(fileContents);
            Dictionary<string, Constant> dotnetYamlConstants = GetYamlDotNetConstants(yamlContents);
            // update dotnet core sdks
            foreach (var platformConstant in platformConstants)
            {
                string version = platformConstant.Version;
                string sha = platformConstant.Sha;
                string versionType = platformConstant.VersionType;
                string dotNetConstantKey = GenerateDotNetConstantKey(version, versionType);
                Console.WriteLine($"version: {version} versionType: {versionType} sha: {sha} dotNetConstantKey: {dotNetConstantKey}");
                if (versionType.Equals("sdk"))
                {
                    Constant dotNetYamlConstant = dotnetYamlConstants["dot-net-core-sdk-versions"];
                    dotNetYamlConstant.Constants[dotNetConstantKey] = version;

                    // add to versionsToBuild.txt
                }
                else
                {
                    Constant dotNetYamlConstant = dotnetYamlConstants["dot-net-core-run-time-versions"];
                    dotNetYamlConstant.Constants[dotNetConstantKey] = version;
                    // store SHAs for net-core and aspnet-core
                    dotNetYamlConstant.Constants[$"{dotNetConstantKey}-sha"] = sha;
                }
            }

            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var stringResult = serializer.Serialize(yamlContents);
            Console.WriteLine($"stringResult: \n{stringResult}");
            File.WriteAllText("build/constants.yaml", stringResult);
        }

        private static Dictionary<string, Constant> GetYamlDotNetConstants(List<Constant> yamlContents)
        {
            Dictionary<string, Constant> dotNetConstants = new Dictionary<string, Constant>();
            foreach (var constant in yamlContents)
            {
                if (constant.Name == "dot-net-core-sdk-versions" ||
                    constant.Name == "dot-net-core-run-time-versions")
                {
                    dotNetConstants.Add(constant.Name, constant);
                }
            }
            return dotNetConstants;
        }

        private static string GenerateDotNetConstantKey(string version, string versionType)
        {
            string constant = string.Empty;
            string[] splitVersion = version.Split('.');
            string majorVersion = splitVersion[0];
            string minorVersion = splitVersion[1];
            // Console.WriteLine($"GenerateConstant version: {version}");
            // Console.WriteLine($"majorVersion: {majorVersion} minorVersion: {minorVersion}");
            string majorMinor = majorVersion + minorVersion;
            if (versionType.Equals("sdk"))
            {
                // TODO: add try catch in case the integer is un-parseable.
                int majorVersionInt = int.Parse(majorVersion);
                string prefix = majorVersionInt < 5 ? $"dot-net-core" : "dot-net";
                constant = $"{prefix}-{majorMinor}-sdk-version";
            }
            else {
                constant = $"{versionType}-app-{majorMinor}";
            }
            Console.WriteLine($"GenerateConstant: {constant}");
            return constant;
        }

        private static bool ReleasedToday(string date)
        {
            var dateReleased = DateTime.Parse(date);
            var dateToday = DateTime.Today;
            int releasedToday = DateTime.Compare(dateReleased, dateToday);
            //Console.WriteLine($"releasedToday: {releasedToday} " +
            //    $"dateReleased: {dateReleased} dateNow: {dateToday}");

            // return releasedToday == 0;
            string today = "2022-08-09";
            bool match = date == today;
            // Console.WriteLine($"today: {today} date: {date} match: {match}");
            return match;
        }

        private static string GetSha(List<FileObj> files)
        {
            HashSet<string> tarFileNames = new HashSet<string>() {
                "dotnet-sdk-linux-x64.tar.gz",
                "dotnet-runtime-linux-x64.tar.gz",
                "aspnetcore-runtime-linux-x64.tar.gz",
            };
            foreach (var file in files)
            {
                if (tarFileNames.Contains(file.Name))
                {
                    return file.Hash;
                }
            }

            Console.WriteLine($"No sha found");
            // TODO: throw exception if not found

            return string.Empty;
        }

        private class ReleaseNotes
        {
            [JsonProperty(PropertyName = "releases-index")]
            public List<ReleaseNote>? ReleasesIndex { get; set; }
        }

        private class ReleaseNote
        {
            [JsonProperty(PropertyName = "channel-version")]
            public string ChannelVersion { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "latest-release")]
            public string LatestRelease { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "latest-release-date")]
            public string LatestReleaseDate { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "security")]
            public string Security { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "latest-runtime")]
            public string LatestRuntime { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "latest-sdk")]
            public string LatestSdk { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "product")]
            public string Product { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "support-phase")]
            public string SupportPhase { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "eol-date")]
            public string EolDate { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "releases.json")]
            public string ReleasesJson { get; set; } = string.Empty;
        }

        private class FileObj
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; } = string.Empty;
            [JsonProperty(PropertyName = "rid")]
            public string Rid { get; set; } = string.Empty;
            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; } = string.Empty;
            [JsonProperty(PropertyName = "hash")]
            public string Hash { get; set; } = string.Empty;
        }

        private class Sdk
        {
            [JsonProperty(PropertyName = "version")]
            public string Version { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "version-display")]
            public string VersionDisplay { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "runtime-version")]
            public string RuntimeVersion { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "files")]
            public List<FileObj> Files { get; set; } = new List<FileObj>();
        }

        private class Runtime
        {
            [JsonProperty(PropertyName = "version")]
            public string Version { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "version-display")]
            public string VersionDisplay { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "files")]
            public List<FileObj> Files { get; set; } = new List<FileObj>();
        }

        private class AspnetCoreRuntime
        {
            [JsonProperty(PropertyName = "version")]
            public string Version { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "version-display")]
            public string VersionDisplay { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "files")]
            public List<FileObj> Files { get; set; } = new List<FileObj>();
        }

        private class Release
        {
            [JsonProperty(PropertyName = "release-date")]
            public string ReleaseDate { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "sdk")]
            public Sdk Sdk { get; set; } = new Sdk();

            [JsonProperty(PropertyName = "runtime")]
            public Runtime Runtime { get; set; } = new Runtime();

            [JsonProperty(PropertyName = "aspnetcore-runtime")]
            public AspnetCoreRuntime AspnetCoreRuntime { get; set; } = new AspnetCoreRuntime();
        }

        private class ReleasesJson // TODO: come up with a better name
        {
            [JsonProperty(PropertyName = "releases")]
            public List<Release> Releases { get; set; } = new List<Release>();
        }

        private class Constant
        {
            public string? Name { get; set; }

            public Dictionary<string, object> Constants { get; set; } = new Dictionary<string, object>();

            public List<object> Outputs { get; set; } = new List<object>();
        }
    }
}