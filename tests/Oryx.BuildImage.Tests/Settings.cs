// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace Microsoft.Oryx.BuildImage.Tests
{
    internal static class Settings
    {
        public const string BuildImageName = "oryxtests/build:stretch";
        public const string BuildImageWithRootAccess = "oryx/build:stretch";
        public const string LtsVersionsBuildImageName = "oryxtests/build:lts-versions-stretch";
        public const string LtsVersionsBuildImageWithRootAccess = "oryx/build:lts-versions-stretch";

        public const string OryxVersion = "0.2.";

        public const string MySqlDbImageName = "mysql/mysql-server:5.7";
        public const string PostgresDbImageName = "postgres";

        public static readonly OSPlatform LinuxOS = OSPlatform.Create("LINUX");
    }
}