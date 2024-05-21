// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class SdkModel
    {
        public string Version { get; set; }

        // From spec:
        // If you don't set this value explicitly, the default value depends on whether you're running from
        // Visual Studio:
        // If you're not in Visual Studio, the default value is true.
        public bool AllowPreRelease { get; set; } = true;

        // From spec: If no rollFoward value is set, it uses latestPatch as the default rollForward policy
        public RollForwardPolicy RollForward { get; set; } = RollForwardPolicy.LatestPatch;
    }
}
