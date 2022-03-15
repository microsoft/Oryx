// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public interface IDotNetCoreVersionProvider
    {
        /// <summary>
        /// Gets a list of supported versions.
        /// The keys represent 'runtime' version, whereas the values represent 'sdk version'.
        /// </summary>
        /// <returns>The list of supported .NET Core versions.</returns>
        Dictionary<string, string> GetSupportedVersions();

        string GetDefaultRuntimeVersion();
    }
}