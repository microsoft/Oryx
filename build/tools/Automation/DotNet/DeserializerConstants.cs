// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Oryx.Automation
{
    /// <summary>
    /// The following classes:
    ///     - ReleaseNotes
    ///     - ReleaseNote
    ///  Are used to deserialize DotNet release meta data.
    ///  Meta data URL: https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "This class shares part of the deserialization functionality.")]
    public class ReleaseNotes
    {
        [JsonProperty(PropertyName = "releases-index")]
        public List<ReleaseNote> ReleasesIndex { get; set; } = new List<ReleaseNote>();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This class shares part of the deserialization functionality.")]
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
        public string ReleasesJsonUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// The following classes:
    ///     - FileObj
    ///     - Sdk
    ///     - RuntimeDotNet
    ///     - AspNetCoreRuntime
    ///     - Release
    ///     - ReleaseJson
    ///  Are used to deserialize correspoding dotnet version release json.
    ///  Example:
    ///  https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/7.0/releases.json
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This class shares part of the deserialization functionality.")]
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This class shares part of the deserialization functionality.")]
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This class shares part of the deserialization functionality.")]
    public class RuntimeDotNet
    {
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "version-display")]
        public string VersionDisplay { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "files")]
        public List<FileObj> Files { get; set; } = new List<FileObj>();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This class shares part of the deserialization functionality.")]
    public class AspnetCoreRuntime
    {
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "version-display")]
        public string VersionDisplay { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "files")]
        public List<FileObj> Files { get; set; } = new List<FileObj>();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This class shares part of the deserialization functionality.")]
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This class shares part of the deserialization functionality.")]
    public class ReleasesJson
    {
        [JsonProperty(PropertyName = "releases")]
        public List<Release> Releases { get; set; } = new List<Release>();
    }
}