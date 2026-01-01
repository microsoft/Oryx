// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Oryx.Tests.Common
{
    public static class TestValueGenerator
    {
        private readonly static List<(string Version, string OsType)> NodeVersions = new List<(string, string)>
        {
            ("20", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("20", ImageTestHelperConstants.OsTypeDebianBookworm),
            ("22", ImageTestHelperConstants.OsTypeDebianBookworm),
        };

        private readonly static List<(string Version, string OsType)> NodeBullseyeVersions = new List<(string, string)>
        {
            ("20", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("22", ImageTestHelperConstants.OsTypeDebianBullseye),
        };

        private readonly static List<(string Version, string OsType)> NodeBookwormVersions = new List<(string, string)>
        {
            ("20", ImageTestHelperConstants.OsTypeDebianBookworm),
            ("22", ImageTestHelperConstants.OsTypeDebianBookworm),
        };

        private readonly static List<(string Version, string OsType)> PythonVersions = new List<(string, string)>
        {
            ("3.7", ImageTestHelperConstants.OsTypeDebianBuster),
            ("3.7", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("3.8", ImageTestHelperConstants.OsTypeDebianBuster),
            ("3.8", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("3.9", ImageTestHelperConstants.OsTypeDebianBuster),
            ("3.9", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("3.10", ImageTestHelperConstants.OsTypeDebianBuster),
            ("3.10", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("3.11", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("3.11", ImageTestHelperConstants.OsTypeDebianBookworm),
            ("3.12", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("3.12", ImageTestHelperConstants.OsTypeDebianBookworm),
            ("3.13", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("3.13", ImageTestHelperConstants.OsTypeDebianBookworm),
        };

        private readonly static List<(string Version, string OsType)> PythonBusterVersions = new List<(string, string)>
        {
            ("3.7", ImageTestHelperConstants.OsTypeDebianBuster),
            ("3.8", ImageTestHelperConstants.OsTypeDebianBuster),
            ("3.9", ImageTestHelperConstants.OsTypeDebianBuster),
        };

        private readonly static List<(string Version, string OsType)> PythonBullseyeVersions = new List<(string, string)>
        {
            ("3.7", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("3.8", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("3.9", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("3.10", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("3.11", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("3.12", ImageTestHelperConstants.OsTypeDebianBullseye),
            ("3.13", ImageTestHelperConstants.OsTypeDebianBullseye),
        };

        public static IEnumerable<object[]> GetNodeVersions_SupportDebugging()
        {
            var versions = new List<(string Version, string OsType)>
            {
                ("14", ImageTestHelperConstants.OsTypeDebianBuster),
                ("14", ImageTestHelperConstants.OsTypeDebianBullseye),
                ("16", ImageTestHelperConstants.OsTypeDebianBuster),
                ("16", ImageTestHelperConstants.OsTypeDebianBullseye)
            };

            return versions.Select(x => new object[] { x.Version, x.OsType });
        }

        public static IEnumerable<object[]> GetBullseyeNodeVersions_SupportDebugging()
        {
            var versions = new List<(string Version, string OsType)>
            {
                ("20", ImageTestHelperConstants.OsTypeDebianBullseye)
            };

            return versions.Select(x => new object[] { x.Version, x.OsType });
        }

        public static IEnumerable<object[]> GetNodeVersions()
        {
            foreach (var x in NodeVersions)
            {
                yield return new object[] { x.Version, x.OsType };
            }
        }

        public static IEnumerable<object[]> GetBullseyeNodeVersions()
        {
            foreach (var x in NodeBullseyeVersions)
            {
                yield return new object[] { x.Version, ImageTestHelperConstants.OsTypeDebianBullseye };
            }
        }

        public static IEnumerable<object[]> GetBookwormNodeVersions()
        {
            foreach (var x in NodeBookwormVersions)
            {
                yield return new object[] { x.Version, ImageTestHelperConstants.OsTypeDebianBookworm };
            }
        }

        public static IEnumerable<object[]> GetPythonVersions()
        {
            foreach (var x in PythonVersions)
            {
                yield return new object[] { x.Version, x.OsType };
            }
        }

        public static IEnumerable<object[]> GetBusterPythonVersions()
        {
            foreach (var x in PythonBusterVersions)
            {
                yield return new object[] { x.Version, ImageTestHelperConstants.OsTypeDebianBuster };
            }
        }

        public static IEnumerable<object[]> GetBullseyePythonVersions()
        {
            foreach (var x in PythonBullseyeVersions)
            {
                yield return new object[] { x.Version, ImageTestHelperConstants.OsTypeDebianBullseye };
            }
        }

        public static IEnumerable<object[]> GetNodeVersions_SupportPm2()
        {
            return NodeVersions
                .Select(x => new object[] { x.Version, x.OsType });
        }
        
        public static IEnumerable<object[]> GetBullseyeNodeVersions_SupportPm2()
        {
            return NodeBullseyeVersions
                .Select(x => new object[] { x.Version, ImageTestHelperConstants.OsTypeDebianBullseye });
        }

        public static IEnumerable<object[]> GetBookwormNodeVersions_SupportPm2()
        {
            return NodeBookwormVersions
                .Select(x => new object[] { x.Version, ImageTestHelperConstants.OsTypeDebianBookworm });
        }
    }
}