// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    using SemVer;

    internal class NodeVersionProvider : INodeVersionProvider
    {
        private string[] _supportedNodeVersions;
        private string[] _supportedNpmVersions;

        // TODO: dynamically get these values from the image, so we don't have
        // to duplicate this info in multiple places
        private static string[] SupportedNodeVersions = new string[]
        {
            "4.4.7",
            "4.5.0",
            "6.2.2",
            "6.6.0",
            "6.9.3",
            "6.10.3",
            "6.11.0",
            "8.0.0",
            "8.1.0",
            "8.2.1",
            "8.8.1",
            "8.9.4",
            "8.12.2",
            "9.4.0",
            "10.1.0",
        };

        // TODO: dynamically get these values from /opt/npm/ from our build image.
        private static string[] SupportedNpmVersions = new string[]
        {
            "2.15.8",
            "2.15.9",
            "3.10.10",
            "3.10.3",
            "3.9.5",
            "5.0.0",
            "5.0.3",
            "5.3.0",
            "5.4.2",
            "5.6.0",
        };

        public NodeVersionProvider(string[] supportedNodeVersions, string[] supportedNpmVersions)
        {
            _supportedNodeVersions = supportedNodeVersions;
            _supportedNpmVersions = supportedNpmVersions;
        }

        public NodeVersionProvider() : this(SupportedNodeVersions, SupportedNpmVersions)
        {
        }

        /// <summary>
        /// <see cref="INodeVersionProvider.GetSupportedNodeVersion(string)(string)"/>
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
    }
}