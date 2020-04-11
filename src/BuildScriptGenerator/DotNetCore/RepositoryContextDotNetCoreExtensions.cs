// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public partial class RepositoryContext
    {
        /// <summary>
        /// Gets or sets the version of .NET Core used in the repo.
        /// </summary>
        public string ResolvedDotNetCoreRuntimeVersion { get; set; }
    }
}
