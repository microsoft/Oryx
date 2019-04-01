// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal interface IPhpVersionProvider
    {
        IEnumerable<string> SupportedPhpVersions { get; }
    }
}