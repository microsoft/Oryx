// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal class NodeVersionProvider : INodeVersionProvider
    {
        private IEnumerable<string> _supportedNodeVersions;
        private IEnumerable<string> _supportedNpmVersions;

        internal const string NodeJsVersionsDir = "/opt/nodejs/";
        internal const string NpmJsVersionsDir = "/opt/npm/";

        public IEnumerable<string> SupportedNodeVersions
        {
            get
            {
                if (_supportedNodeVersions == null)
                {
                    _supportedNodeVersions = GetSupportedNodeVersionsFromImage();
                }
                return _supportedNodeVersions;
            }
        }

        public IEnumerable<string> SupportedNpmVersions
        {
            get
            {
                if (_supportedNpmVersions == null)
                {
                    _supportedNpmVersions = GetSupportedNpmVersionsFromImage();
                }
                return _supportedNpmVersions;
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
            var versions = GetVersionsFromDirectory(versionsDir);
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

            // Since the directories contain folders (or links) like 'lts' and 'latest', try getting
            // proper version using wildcards.
            var versionDirectories = Directory.EnumerateDirectories(versionsDir, "*.*.*", listOptions)
                .Select(versionDir => new DirectoryInfo(versionDir));
            foreach (var versionDir in versionDirectories)
            {
                versions.Add(versionDir.Name);
            }

            return versions;
        }
    }
}