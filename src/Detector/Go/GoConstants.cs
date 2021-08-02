// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector.Go
{
    internal static class GoConstants
    {
        internal const string PlatformName = "go";
        internal const string GoDotModFileName = "go.mod";
        public static readonly string[] IisStartupFiles = new[]
        {
            "go.mod"
        };
    }
}
