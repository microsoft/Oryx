// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class DotNetCoreScriptGeneratorOptions
    {
        /// <summary>
        /// Gets or sets the MSBuild configuration that needs to be used when doing 'dotnet build'.
        /// </summary>
        public string MSBuildConfiguration { get; set; }

        /// <summary>
        /// Gets or sets custom build command that will run instead of the default
        /// 'dotnet restore' and 'dotnet publish' in the generated build script.
        /// </summary>
        public string CustomBuildCommand { get; set; }

        public string DotNetCoreRuntimeVersion { get; set; }

        public string DefaultRuntimeVersion { get; set; }
    }
}