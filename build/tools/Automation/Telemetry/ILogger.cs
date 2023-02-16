// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.Automation.Telemetry
{
    public interface ILogger
    {
        ILogger<DotNet.DotNet> Logger { get; }
    }
}
