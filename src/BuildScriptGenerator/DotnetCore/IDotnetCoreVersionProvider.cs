// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.DotnetCore
{
    /// <summary>
    /// Abstraction listing the supported .NET Core versions.
    /// </summary>
    public interface IDotnetCoreVersionProvider
    {
        IEnumerable<string> SupportedVersions { get; }
    }
}
