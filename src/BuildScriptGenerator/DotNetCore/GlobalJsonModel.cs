// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class GlobalJsonModel
    {
        public Sdk Sdk { get; set; }
    }

    public class Sdk
    {
        public string Version { get; set; }

        public string AllowPreRelease { get; set; }

        public string RollForward { get; set; }
    }
}
