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
        public override async Task<List<PlatformConstant>> GetPlatformConstantsAsync()
        {

            // get dotnet releases' meta data
            string releasesUrl = "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json";
            var response = await Request.RequestAsync(releasesUrl);
            var json = JsonConvert.DeserializeObject<ReleaseNotes>(response);
            var releasesMetaData = json == null ? new List<ReleaseNote>() : json.ReleasesIndex;
            List<PlatformConstant> platformConstants = new List<PlatformConstant>();
            foreach (var releaseMetaData in releasesMetaData)
            {
                // get releases that are released today
                var dateReleased = releaseMetaData.LatestReleaseDate;
                if (!ReleasedToday(dateReleased)) continue;
                string releasesJsonUrl = releaseMetaData.ReleasesJson;
                response = await Request.RequestAsync(releasesJsonUrl);
                var releasesJson = JsonConvert.DeserializeObject<ReleasesJson>(response);
                var releases = releasesJson == null ? new List<Release>() : releasesJson.Releases;
                foreach (var release in releases)
                {
                    if (!ReleasedToday(release.ReleaseDate)) continue;
                    Console.WriteLine($"release-date: {release.ReleaseDate}");

                    // update sdk
                    string sdkVersion = release.Sdk.Version;
                    string sha = GetSha(release.Sdk.Files);
                    PlatformConstant platformConstant = new PlatformConstant
                    {
                        Version = sdkVersion,
                        Sha = sha,
                        PlatformName = "dotnet",
                        VersionType = "sdk",
                    };
                    platformConstants.Add(platformConstant);

                    // update runtime (netcore)
                    string runtimeVersion = release.Runtime.Version;
                    sha = GetSha(release.Runtime.Files);
                    Console.WriteLine($"For Runtime: {runtimeVersion} {release.Runtime.VersionDisplay} {sha}");
                    platformConstant = new PlatformConstant
                    {
                        Version = runtimeVersion,
                        Sha = sha,
                        PlatformName = "dotnet",
                        VersionType = "net-core",
                    };
                    platformConstants.Add(platformConstant);

                    // update runtime (aspnetcore)
                    string aspnetCoreRuntimeVersion = release.AspnetCoreRuntime.Version;
                    sha = GetSha(release.AspnetCoreRuntime.Files);
                    Console.WriteLine($"For AspnetCoreRuntime: {aspnetCoreRuntimeVersion} {release.AspnetCoreRuntime.VersionDisplay} {sha}");
                    platformConstant = new PlatformConstant
                    {
                        Version = aspnetCoreRuntimeVersion,
                        Sha = sha,
                        PlatformName = "dotnet",
                        VersionType = "aspnet-core",
                    };
                    platformConstants.Add(platformConstant);
                }
            }

            return platformConstants;
        }

        /// <inheritdoc/>
        public override void UpdateConstants(List<PlatformConstant> platformConstants, List<Constant> yamlConstants)
        {
            Dictionary<string, Constant> dotnetYamlConstants = GetYamlDotNetConstants(yamlConstants);

            // update dotnetcore sdks and runtimes
            foreach (var platformConstant in platformConstants)
            {
                string version = platformConstant.Version;
                string sha = platformConstant.Sha;
                string versionType = platformConstant.VersionType;
                string dotNetConstantKey = GenerateDotNetConstantKey(platformConstant);
                // Console.WriteLine($"version: {version} versionType: {versionType} sha: {sha} dotNetConstantKey: {dotNetConstantKey}");
                if (versionType.Equals("sdk"))
                {
                    Constant dotNetYamlConstant = dotnetYamlConstants["dot-net-core-sdk-versions"];
                    dotNetYamlConstant.Constants[dotNetConstantKey] = version;

                    // add sdk to versionsToBuild.txt
                    UpdateVersionsToBuildTxt(platformConstant);
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

            var stringResult = serializer.Serialize(yamlConstants);
            //Console.WriteLine($"stringResult: \n{stringResult}");
            File.WriteAllText(Constants.ConstantsYaml, stringResult);
        }

        private static void UpdateVersionsToBuildTxt(PlatformConstant platformConstant)
        {
            Console.WriteLine("Updating versionToBuild.txt");
            List<string> versionsToBuildTxtFiles = new List<string>() {
                    "platforms/dotnet/versions/bullseye/versionsToBuild.txt",
                    "platforms/dotnet/versions/buster/versionsToBuild.txt",
                    "platforms/dotnet/versions/focal-scm/versionsToBuild.txt",
                    "platforms/dotnet/versions/stretch/versionsToBuild.txt",
            };
            foreach (string versionsToBuildTxtFile in versionsToBuildTxtFiles)
            {
                string line = $"\n{platformConstant.Version}, {platformConstant.Sha},";
                File.AppendAllText(versionsToBuildTxtFile, line);

                // sort
                var contents = File.ReadAllLines(versionsToBuildTxtFile);
                Array.Sort(contents);
                File.WriteAllLines(versionsToBuildTxtFile, contents);
            }
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

        private static string GenerateDotNetConstantKey(PlatformConstant platformConstant)
        {
            string[] splitVersion = platformConstant.Version.Split('.');
            string majorVersion = splitVersion[0];
            string minorVersion = splitVersion[1];
            // Console.WriteLine($"GenerateConstant version: {version}");
            // Console.WriteLine($"majorVersion: {majorVersion} minorVersion: {minorVersion}");
            string majorMinor = majorVersion + minorVersion;
            string constant;
            if (platformConstant.VersionType.Equals("sdk"))
            {
                // TODO: add try catch in case the integer is un-parseable.
                int majorVersionInt = int.Parse(majorVersion);
                string prefix = majorVersionInt < 5 ? $"dot-net-core" : "dot-net";
                constant = $"{prefix}-{majorMinor}-sdk-version";
            }
            else
            {
                constant = $"{platformConstant.VersionType}-app-{majorMinor}";
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
            string today = "2022-09-13";
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
    }
}