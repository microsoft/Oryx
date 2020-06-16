// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class DotNetCoreScriptGeneratorOptions
    {
        /// <summary>
        /// Gets or sets the relative path to the project to be built.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Gets or sets the MSBuild configuration that needs to be used when doing 'dotnet build'.
        /// </summary>
        public string MSBuildConfiguration { get; set; }

        public string DotNetCoreRuntimeVersion { get; set; }
    }
}