// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Golang
{
    public static class GolangConstants
    {
        public const string PlatformName = "golang";
        public const string GoModFileName = "go.mod";
        public const string GolangDefaultVersion = "1.16";
        public const string InstalledGolangVersionsDir = "/opt/golang/";
        public const string DynamicInstalledGolangVersionsDir = "/tmp/oryx/platforms/golang";
        public const string GolangVersionEnvVarName = "GOLANG_VERSION";
    }
}
