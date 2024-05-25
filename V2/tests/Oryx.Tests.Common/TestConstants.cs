// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.Tests.Common
{
    public static class TestConstants
    {
        public const string LinuxPlatform = "LINUX";

        // Xunit trait values
        public const string Category = "Category";
        public const string Release = "Release";

        //AI key 
        public const string AppInsightsConnectionString = "InstrumentationKey=test";
        public const string AppInsightsConnectionStringEnvironmentVariable = "TEST_AI_CONNECTION_STRING";
    }
}
