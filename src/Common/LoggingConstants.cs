// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Oryx.Common
{
    public static class LoggingConstants
    {
        public const string ApplicationInsightsInstrumentationKeyEnvironmentVariableName
            = "ORYX_AI_INSTRUMENTATION_KEY";

        public const string DefaultOperationName = ".oryx";

        public const string DefaultLogPath = "/tmp/oryx.log";

        public static readonly IDictionary<string, string> OperationNameSourceEnvVars = new Dictionary<string, string>
        {
            { ExtVarNames.AppServiceAppNameEnvVarName, "AAS" },
            { "REGISTRY_NAME", "ACR" }
        };

        public static readonly TimeSpan FlushTimeout = TimeSpan.FromSeconds(3);
    }
}
