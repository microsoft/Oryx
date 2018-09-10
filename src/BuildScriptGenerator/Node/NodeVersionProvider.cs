// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    using System.Collections.Generic;
    using System.IO;
    using SemVer;

    internal class NodeVersionProvider : INodeVersionProvider
    {
        private const string NodeJsVersionsDir = "/opt/nodejs/";
        private const string NpmJsVersionsDir = "/opt/npm/";

        private IEnumerable<string> _supportedNodeVersions;
        private IEnumerable<string> _supportedNpmVersions;

        public NodeVersionProvider(string[] supportedNodeVersions, string[] supportedNpmVersions)
        {
            _supportedNodeVersions = supportedNodeVersions;
            _supportedNpmVersions = supportedNpmVersions;
        }

        public NodeVersionProvider()
        {
            _supportedNodeVersions = GetSupportedNodeVersionsFromImage();
            _supportedNpmVersions = GetSupportedNpmVersionsFromImage();
        }

        /// <summary>
        /// <see cref="INodeVersionProvider.GetSupportedNodeVersion(string)"/>
        /// </summary>
        public string GetSupportedNodeVersion(string versionRange)
        {
            try
            {
                var range = new Range(versionRange);
                var satisfying = range.MaxSatisfying(_supportedNodeVersions);
                return satisfying;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// <see cref="INodeVersionProvider.GetSupportedNpmVersion(string)"/>
        /// </summary>
        public string GetSupportedNpmVersion(string versionRange)
        {
            try
            {
                var range = new Range(versionRange);
                var satisfying = range.MaxSatisfying(_supportedNpmVersions);
                return satisfying;
            }
            catch
            {
                return null;
            }
        }

        private IEnumerable<string> GetSupportedNodeVersionsFromImage()
        {
            const string versionsDir = NodeJsVersionsDir;
            var versions = GetVersionsFromDirectory(versionsDir);

            return versions;
        }

        private IEnumerable<string> GetSupportedNpmVersionsFromImage()
        {
            const string versionsDir = NpmJsVersionsDir;
            List<string> versions = GetVersionsFromDirectory(versionsDir);

            return versions;
        }

        private static List<string> GetVersionsFromDirectory(string versionsDir)
        {
            var listOptions = new EnumerationOptions()
            {
                RecurseSubdirectories = false,
                IgnoreInaccessible = false,
            };
            var versions = new List<string>();

            var nodeJsVersionDirs = System.IO.Directory.EnumerateDirectories(versionsDir, "*.*.*", listOptions);
            foreach (var vDir in nodeJsVersionDirs)
            {
                var versionNumber = vDir.Substring(versionsDir.Length);
                versions.Add(vDir);
            }

            return versions;
        }
    }
}