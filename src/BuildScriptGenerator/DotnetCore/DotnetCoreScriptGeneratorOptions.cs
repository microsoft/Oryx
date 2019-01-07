// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.DotnetCore
{
    public class DotnetCoreScriptGeneratorOptions
    {
        public string DefaultVersion { get; set; }

        public string InstalledVersionsDir { get; set; }

        public IList<string> SupportedVersions { get; set; }
    }
}