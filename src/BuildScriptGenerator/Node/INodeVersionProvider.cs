// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal interface INodeVersionProvider
    {
        IEnumerable<string> SupportedNodeVersions { get; }

        IEnumerable<string> SupportedNpmVersions { get; }
    }
}