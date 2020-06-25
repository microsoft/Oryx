// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public enum RollForwardPolicy
    {
        Disable,
        Patch,
        Feature,
        Minor,
        Major,
        LatestPatch,
        LatestFeature,
        LatestMinor,
        LatestMajor,
    }
}
