// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.Oryx.Automation
{
    /// <Summary>
    ///
    /// TODO:
    ///     - Replace Console.WriteLine with Logging
    ///     - Add unit tests
    ///
    /// Helps automate detecting and releasing new SDK versions for Oryx.
    ///
    /// Limitation:
    ///     When a new major version is released we need to manually update our tests/
    ///     with the new constant that gets generated.
    ///     For example:
    ///         If DotNetCore whenever dotnet 8 gets released, a 'DOT_NET_80_SDK_VERSION'
    ///         constant will get generated. This constant will need to be updated in
    ///         our tests/ folder.
    /// </Summary>
    public abstract class Program
    {
        private static string repoAbsolutePath = string.Empty;
        private static HashSet<string> prodSdkVersions = new HashSet<string>();

        // private static HashSet<string> sdkVersions = new HashSet<string>();
        public static async Task<int> Main(string[] args)
        {
            // TODO: use dotnet parameters instead and handle invalid date
            if (args.Length != 2)
            {
                Console.WriteLine("[Main] Error: missing 2 required arguments.\n" +
                    "1. String date target in format: yyyy-mm-dd\n" +
                    "2. String repo absolute path to the root of Oryx repo.\n");
                Environment.Exit(1);
            }

            string dateTarget = args[0];
            repoAbsolutePath = args[1];

            Console.WriteLine($"[Main] dateTarget: {dateTarget}");
            await CacheSdkVersionsAsync("dotnet");
            await AddNewPlatformConstantsAsync(dateTarget);

            return 0;
        }

        /// <Summary>
        /// Adds new platform constants to Oryx repo
        /// TODO: use dependency injection
        /// </Summary>
        public static async Task AddNewPlatformConstantsAsync(string dateTarget)
        {
            DotNet dotNet = new DotNet(repoAbsolutePath, prodSdkVersions);
            List<PlatformConstant> platformConstants = await dotNet.GetPlatformConstantsAsync(dateTarget);
            List<Constant> yamlConstants = await DeserializeConstantsYamlAsync();
            dotNet.UpdateConstants(platformConstants, yamlConstants);

            // TODO: add functionality for other platforms (python, java, golang, etc).
        }

        /// <Summary>
        /// Updates:
        ///     - Constants.ConstantsYaml
        ///
        /// Deserializes Constants.ConstantsYaml for platforms, that have new releases, can update.
        /// </Summary>
        public static async Task<List<Constant>> DeserializeConstantsYamlAsync()
        {
            var constantsYamlAbsolutePath = Path.Combine(repoAbsolutePath, "build", Constants.ConstantsYaml);
            string fileContents = await File.ReadAllTextAsync(constantsYamlAbsolutePath).ConfigureAwait(true);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            var yamlContents = deserializer.Deserialize<List<Constant>>(fileContents);
            return yamlContents;
        }

        public static bool DatesMatch(string dateTarget, string dateReleased)
        {
            var releasedDate = DateTime.Parse(dateReleased);
            var targetDate = DateTime.Parse(dateTarget);
            int datesMatch = DateTime.Compare(releasedDate, targetDate);
            bool match = datesMatch == 0;
            Console.WriteLine($"[DatesMatch]  releasedDate: {releasedDate} targetDate: {targetDate} " +
                $"datesMatch: {datesMatch} match: {match}");
            return match;
        }

        public static async Task CacheSdkVersionsAsync(string platform)
        {
            string url = $"https://oryxsdks.blob.core.windows.net/{platform}" +
            "?restype=container&comp=list&include=metadata";
            var response = await HttpClientHelper.GetRequestStringAsync(url);
            var xdoc = XDocument.Parse(response);

            foreach (var metadataElement in xdoc.XPathSelectElements($"//Blobs/Blob/Metadata"))
            {
                var childElements = metadataElement.Elements();
                var versionElement = childElements
                                    .Where(e => string.Equals("sdk_version", e.Name.LocalName, StringComparison.OrdinalIgnoreCase))
                                    .FirstOrDefault();
                if (versionElement != null)
                {
                    prodSdkVersions.Add(versionElement.Value);
                    Console.WriteLine(versionElement.Value);
                }
            }
        }

        /// <Summary>
        /// Get PlatformConstants containing corresponding platform release information.
        /// Release information such as version, sha, etc.
        /// An empty list will be returned if there are no new releases.
        /// </Summary>
        /// <param name="dateTarget">yyyy-mm-dd format string that defaults to today's date.
        /// This dateTarget can be passed through github actions through an argument</param>
        /// <returns>PlatformConstants used later to update constants.yaml</returns>
        public abstract Task<List<PlatformConstant>> GetPlatformConstantsAsync(string dateTarget);

        /// <Summary>
        /// Updates:
        ///     - constants.yaml
        ///     - versionsToBuild.txt
        ///
        /// Use PlatformConstants to populate constants.yaml and versionsToBuild.txt files
        /// with relevant new platform release information. The constants.yaml file is populated
        /// so after build/generateConstants.sh is invoked, the contants.yaml is used to distribute changes
        /// across Oryx source code. Which allows tests to be automatically to be updated.
        ///
        /// <param name="platformConstants">List of PlatformConstant containing platform release information</param>
        /// <param name="yamlConstants">Deserialized Constants.ConstantsYaml which is ready for editing</param>
        /// </Summary>
        public abstract void UpdateConstants(List<PlatformConstant> platformConstants, List<Constant> yamlConstants);

        /// <Summary>
        /// Stores platform release information so it can be referenced when updating:
        /// constants.yaml and versionsToBuild.txt files
        /// </Summary>
        public class PlatformConstant
        {
            /// <Summary>
            /// The version of the platfom.
            /// Example: 1.2.3
            /// </Summary>
            public string Version { get; set; } = string.Empty;

            /// <Summary>
            /// The sha of the platform's version.
            /// Some platforms may not have a sha.
            /// </Summary>
            public string Sha { get; set; } = string.Empty;

            /// <Summary>
            /// The name of the platform.
            /// Example: dotnet, golang, etc.
            /// </Summary>
            public string PlatformName { get; set; } = string.Empty;

            /// <Summary>
            /// The type of version that is being represented.
            /// Example: sdk, aspnetcore, netcore, etc.
            /// </Summary>
            public string VersionType { get; set; } = string.Empty;

            // TODO: Add fields for other feilds.
            //  For example, python has GPG keys
        }

        /// <Summary>
        /// This is used to deserialize Constants.ConstantsYaml file
        /// </Summary>
        public class Constant
        {
            public string Name { get; set; } = string.Empty;

            public Dictionary<string, object> Constants { get; set; } = new Dictionary<string, object>();

            public List<object> Outputs { get; set; } = new List<object>();
        }
    }
}