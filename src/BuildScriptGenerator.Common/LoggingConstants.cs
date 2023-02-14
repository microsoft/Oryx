// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    public static class LoggingConstants
    {
        public const string ApplicationInsightsConnectionStringKeyEnvironmentVariableName
            = "ORYX_AI_CONNECTION_STRING";

        public const string OryxDisableTelemetryEnvironmentVariableName
            = "ORYX_DISABLE_TELEMETRY";

        public const string DefaultOperationName = ".oryx";

        public const string DefaultLogPath = "/tmp/oryx.log";

        // Maps from environment variable names to environment type that they imply
        public static readonly IDictionary<EnvironmentType, string> OperationNameSourceEnvVars =
            new Dictionary<EnvironmentType, string>
        {
            { EnvironmentType.AzureAppService,        ExtVarNames.AppServiceAppNameEnvVarName },
            { EnvironmentType.AzureContainerRegistry, "REGISTRY_NAME" },
            { EnvironmentType.VisualStudioOnline,     ExtVarNames.EnvironmentType }, // Currently exported only by VSO
        };

        public static readonly IDictionary<EnvironmentType, string> EnvTypeOperationNamePrefix =
            new Dictionary<EnvironmentType, string>
        {
            { EnvironmentType.AzureAppService,        "AAS" },
            { EnvironmentType.AzureContainerRegistry, "ACR" },
            { EnvironmentType.VisualStudioOnline,     "VSO" }, // Currently exported only by VSO
        };

        public static readonly TimeSpan FlushTimeout = TimeSpan.FromSeconds(3);
    }
}
