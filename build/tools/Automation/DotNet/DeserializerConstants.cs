using Newtonsoft.Json;

namespace Microsoft.Oryx.Automation
{
    public class ReleaseNotes
    {
        [JsonProperty(PropertyName = "releases-index")]
        public List<ReleaseNote> ReleasesIndex { get; set; } = new List<ReleaseNote>();
    }

    public class ReleaseNote
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

    public class FileObj
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

    public class Sdk
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

    public class RuntimeDotNet
    {
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "version-display")]
        public string VersionDisplay { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "files")]
        public List<FileObj> Files { get; set; } = new List<FileObj>();
    }

    public class AspnetCoreRuntime
    {
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "version-display")]
        public string VersionDisplay { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "files")]
        public List<FileObj> Files { get; set; } = new List<FileObj>();
    }


    public class Release
    {
        [JsonProperty(PropertyName = "release-date")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "sdk")]
        public Sdk Sdk { get; set; } = new Sdk();

        [JsonProperty(PropertyName = "runtime")]
        public RuntimeDotNet Runtime { get; set; } = new RuntimeDotNet();

        [JsonProperty(PropertyName = "aspnetcore-runtime")]
        public AspnetCoreRuntime AspnetCoreRuntime { get; set; } = new AspnetCoreRuntime();
    }

    public class ReleasesJson // TODO: come up with a better name
    {
        [JsonProperty(PropertyName = "releases")]
        public List<Release> Releases { get; set; } = new List<Release>();
    }
}