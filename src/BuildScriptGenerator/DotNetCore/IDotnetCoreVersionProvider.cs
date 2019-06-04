// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// Abstraction listing the supported .NET Core versions.
    /// </summary>
    public interface IDotNetCoreVersionProvider
    {
        IEnumerable<string> SupportedDotNetCoreVersions { get; }
    }
}
