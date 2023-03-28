// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class VersionProviderHelper
    {
        internal static IEnumerable<string> GetSupportedVersions(
            IEnumerable<string> optionsVersions,
            string versionsDir)
        {
            return optionsVersions != null ? optionsVersions : GetVersionsFromDirectory(versionsDir);
        }

        internal static IEnumerable<string> GetVersionsFromDirectory(string versionsDir)
        {
            var listOptions = new EnumerationOptions()
            {
                RecurseSubdirectories = false,
                IgnoreInaccessible = false,
            };

            IEnumerable<DirectoryInfo> versionDirectories;
            try
            {
                versionDirectories = Directory.EnumerateDirectories(versionsDir, "*", listOptions)
                    .Select(versionDir => new DirectoryInfo(versionDir));
            }
            catch (IOException)
            {
                return Enumerable.Empty<string>();
            }

            var versions = new List<SemanticVersioning.Version>();
            foreach (var versionDir in versionDirectories)
            {
                try
                {
                    var version = new SemanticVersioning.Version(versionDir.Name);
                    versions.Add(version);
                }
                catch (ArgumentException)
                {
                    // Ignore non-Semantic-Versioning based strings like 'latest' or 'lts'
                }
            }

            versions.Sort();
            return versions.Select(v => v.ToString());
        }

        /// <summary>
        ///     This method is specifically for obtaining Major.Minor version (example: 1.16)
        ///     specifically for Golang since it does not have use semantic-versioning
        ///     As a result we use RegEx extract version instead of SemVer.
        /// </summary>
        internal static IEnumerable<string> GetMajorMinorVersionsFromDirectory(string versionsDir)
        {
            var listOptions = new EnumerationOptions()
            {
                RecurseSubdirectories = false,
                IgnoreInaccessible = false,
            };

            IEnumerable<DirectoryInfo> versionDirectories;
            try
            {
                versionDirectories = Directory.EnumerateDirectories(versionsDir, "*", listOptions)
                    .Select(versionDir => new DirectoryInfo(versionDir));
            }
            catch (IOException)
            {
                return Enumerable.Empty<string>();
            }

            var versions = new List<string>();
            foreach (var versionDir in versionDirectories)
            {
                try
                {
                    /* Regex will match:
                     * major.minor
                     *      where major & minor contains any number of digits
                     * Valid examples:
                     *      1.16, 1.17, 0.1, 123.456
                     * Invalid examples:
                     *      1.16.1, 1.2.3, 1.2a
                    */
                    Regex regex = new Regex(@"^[0-9]+\.[0-9]+$");
                    string version = versionDir.Name;
                    Match match = regex.Match(version);
                    if (match.Success)
                    {
                        versions.Add(version);
                    }
                }
                catch (ArgumentException)
                {
                    // Ignore non-Semantic-Versioning based strings like 'latest' or 'lts'
                }
            }

            return versions;
        }
    }
}