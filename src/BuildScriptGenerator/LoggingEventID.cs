// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class LoggingEventID
    {
        /// <summary>
        /// When user supplied language and version is used.
        /// </summary>
        public const int UserSuppliedPlatformAndVersion = 100;

        /// <summary>
        /// When user supplied language is used but version had to be detected.
        /// </summary>
        public const int UserSuppliedPlatformAndDetectedVersion = 101;
    }
}
