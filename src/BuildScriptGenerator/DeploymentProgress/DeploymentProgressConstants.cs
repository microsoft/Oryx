// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.DeploymentProgress
{
    public static class DeploymentProgressConstants
    {
        public const int SchemaVersion = 1;
        public const int MaxEventsPerStream = 32;
        public const int MaxRecordBytes = 1024;
        public const int MaxFileBytes = 32768;
        public const int MaxOperationIdBytes = 256;
        public const string Capability = "deployment-progress/1";
        public const string EndpointEnvironmentVariable = "ORYX_PROGRESS_ENDPOINT";
        public const string OperationIdPattern = "^[A-Za-z0-9][A-Za-z0-9._:-]{0,255}$";
        public const string OperationIdEnvironmentVariable = "ORYX_PROGRESS_OPERATION_ID";

        internal const string FileEndpointPrefix = "file:";

        internal static readonly IReadOnlyDictionary<string, int> PhaseRanks =
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["script.generate"] = 1,
                ["pre_build"] = 2,
                ["dependencies.restore"] = 3,
                ["build.execute"] = 4,
                ["post_build"] = 5,
                ["output.prepare"] = 6,
                ["output.compress"] = 7,
                ["manifest.write"] = 8,
            };

        internal static readonly ISet<string> BuildOutcomes = new HashSet<string>(StringComparer.Ordinal)
        {
            "succeeded",
            "failed",
            "canceled",
        };
    }
}
