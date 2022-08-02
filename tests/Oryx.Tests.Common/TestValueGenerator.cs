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
        private readonly static List<string> NodeVersions = new List<string>
        {
            "14",
            "16"
        };

        private readonly static List<string> PythonVersions = new List<string>
        {
            "3.7", "3.8", "3.9"
        };

        public static IEnumerable<object[]> GetNodeVersions_SupportDebugging()
        {
            var versions = new List<string>
            {
                "14",
                "16"
            };

            return versions.Select(v => new object[] { v });
        }

        public static IEnumerable<object[]> GetNodeVersions()
        {
            foreach (var version in NodeVersions)
            {
                yield return new object[] { version };
            }
        }
        
        public static IEnumerable<object[]> GetPythonVersions()
        {
            foreach (var version in PythonVersions)
            {
                yield return new object[] { version };
            }
        }

        public static IEnumerable<object[]> GetNodeVersions_SupportPm2()
        {
            return NodeVersions
                .Select(v => new object[] { v });
        }
    }
}