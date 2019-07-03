// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace Microsoft.Oryx.BuildImage.Tests
{
    internal static class Settings
    {
        public const string BuildImageName = "oryxtests/build:latest";
        public const string ProdBuildImageName = "oryxdevms/build:latest";
        public const string OryxVersion = "0.2.";

        public const string Python27Version = "2.7.16";

        public const string MySqlDbImageName = "mysql/mysql-server:5.7";
        public const string PostgresDbImageName = "postgres";

        public static readonly OSPlatform LinuxOS = OSPlatform.Create("LINUX");
    }
}