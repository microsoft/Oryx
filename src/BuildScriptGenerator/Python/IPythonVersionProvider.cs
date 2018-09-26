// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal interface IPythonVersionProvider
    {
        IEnumerable<string> SupportedPythonVersions { get; }
    }
}