// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector.Golang
{
    internal static class GolangConstants
    {
        internal const string PlatformName = "golang";
        internal const string GoDotModFileName = "go.mod";
        public static readonly string[] StartupFiles = new[]
        {
            "go.mod"
        };
    }
}
