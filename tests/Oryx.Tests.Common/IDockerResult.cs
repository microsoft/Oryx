// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Oryx.Tests.Common
{
    interface IDockerResult
    {
        Exception Exception { get; }

        string ExecutedCommand { get; }

        int ExitCode { get; }

        string StdOut { get; }

        string StdErr { get; }

        string GetDebugInfo(IDictionary<string, string> extraDefs = null);
    }
}
