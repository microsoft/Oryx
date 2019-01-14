// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Oryx.Common.Utilities
{
    public static class LoggingConstants
    {
        public const string ApplicationInsightsInstrumentationKeyEnvironmentVariableName = "ORYX_AI_INSTRUMENTATION_KEY";

        public const string AppServiceAppNameEnvironmentVariableName = "APPSETTING_WEBSITE_SITE_NAME";

        public const string DefaultLogPath = "/tmp/oryx.log";

        public static readonly TimeSpan FlushTimeout = TimeSpan.FromSeconds(3);
    }
}
