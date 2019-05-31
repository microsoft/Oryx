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

        // Maps from environment variable names to environment type that they imply
        public static readonly IDictionary<EnvironmentType, string> OperationNameSourceEnvVars =
            new Dictionary<EnvironmentType, string>
        {
            { EnvironmentType.AzureAppService, ExtVarNames.AppServiceAppNameEnvVarName },
            { EnvironmentType.AzureContainerRegistry, "REGISTRY_NAME" }
        };

        public static readonly IDictionary<EnvironmentType, string> EnvTypeOperationNamePrefix =
            new Dictionary<EnvironmentType, string>
        {
            { EnvironmentType.AzureAppService, "AAS" },
            { EnvironmentType.AzureContainerRegistry, "ACR" }
        };

        public static readonly TimeSpan FlushTimeout = TimeSpan.FromSeconds(3);
    }
}
