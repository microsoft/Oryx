// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Detector;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class DotNetCoreScriptGeneratorOptions : DetectorOptions
    {
        /// <summary>
        /// Gets or sets the MSBuild configuration that needs to be used when doing 'dotnet build'.
        /// </summary>
        public string MSBuildConfiguration { get; set; }
    }
}