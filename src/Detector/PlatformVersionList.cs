// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.Detector
{
    public static class PlatformVersionList
    {
        public static string DotNetCoreDefaultVersion = "3.1";
        public static string NodeDefaultVersion = "12.16.3";
        public static string PythonDefaultVersion = "3.8.2";
        public static string PhpDefaultVersion = "7.3.15";

        public static IEnumerable<string> NodeVersionList = new HashSet<string>() {
            "6.2.2",
            "6.17.1",
            "6.11.5",
            "6.11.0",
            "6.10.3",
            "4.8.7",
            "4.8.0",
            "4.5.0",
            "4.4.7",
            "14.0.0",
            "13.9.0",
            "12.9.1",
            "12.16.3",
            "12.16.1",
            "12.16.0",
            "12.14.1",
            "12.14.0",
            "12.13.0",
            "12.12.0",
            "12.11.1",
            "12.11.0",
            "10.19.0",
            "10.18.1",
            "10.18.0",
            "10.16.3",
            "10.14.2",
            "10.12.0",
            "10.10.0",
            "10.1.0",
        };

        public static IEnumerable<string> DotNetCoreVersionList = new HashSet<string>() {
            "1.1.14",
            "2.1.801",
            "2.1.802",
            "2.1.803",
            "2.1.804",
            "2.1.805",
            "2.1.806",
            "2.2.207",
            "2.2.401",
            "2.2.402",
            "3.0.100",
            "3.0.100-preview8-013656",
            "3.0.100-preview9-014004",
            "3.0.100-rc1-014190",
            "3.0.101",
            "3.0.102",
            "3.0.103",
            "3.1.100",
            "3.1.101",
            "3.1.102",
            "3.1.200",
            "3.1.201",
            "3.1.202",
            "5.0.100-preview.1.20155.7",
            "5.0.100-preview.2.20176.6",
            "5.0.100-preview.3.20216.6",
        };

        
        public static IEnumerable<string> PythonVersionList = new HashSet<string>()
        {
            "2.7.16",
            "2.7.17",
            "3.6.10",
            "3.6.9",
            "3.7.3",
            "3.7.4",
            "3.7.5",
            "3.7.6",
            "3.7.7",
            "3.8.0",
            "3.8.0b3",
            "3.8.0b4",
            "3.8.1",
            "3.8.2",
            "3.8.3",
            "3.9.0a6",
            "3.9.0b1",
        };

        public static IEnumerable<string> PhpVersionList = new HashSet<string>()
        {
            "5.6.40",
            "7.0.33",
            "7.2.18",
            "7.2.25",
            "7.2.26",
            "7.2.28",
            "7.3.12",
            "7.3.13",
            "7.3.15",
            "7.3.5",
            "7.4.3",
        };
    }
}
