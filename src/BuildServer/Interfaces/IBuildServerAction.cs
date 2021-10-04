// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using RunProcessAsTask;
using System;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildServer.Interfaces
{
    public interface IBuildServerAction
    {
        Task RunCommandAsync();
        StatusUrl GetStatusUrls(HttpRequest requestContext);
        Task<ProcessResults> RunCommandAsync(string script, TimeSpan timeout);
    }
}
