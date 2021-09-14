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
        (StatusUrl statusUrl, string manifestPath, string logFilePath) GetStatusUrls(HttpRequest requestContext, BuildServerRequests requestBody = null);
        StatusUrl GetBuildStatusUrl(HttpRequest requestContext, string outDir = null);
        Task<ProcessResults> RunCommandAsync(string script, TimeSpan timeout);
    }
}
