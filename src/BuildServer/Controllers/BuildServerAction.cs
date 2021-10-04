// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildServer.Interfaces;
using RunProcessAsTask;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.BuildServer.Controllers
{
    public class BuildServerAction : IBuildServerAction
    {
        public Task RunCommandAsync()
        {
            throw new NotImplementedException();
        }

        public StatusUrl GetStatusUrls(HttpRequest requestContext)
        {
            var result = new StatusUrl();
            var host = requestContext.Host.Value;
            var scheme = requestContext.Scheme;

            string serverUrl = string.Concat(scheme, "://", host, "/build/", "CheckServerStatus");
            result.ServerStatusCheckUrl = serverUrl;

            return result;
        }

        public async Task<ProcessResults> RunCommandAsync(string script, TimeSpan timeout)
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            try
            {
                var processResults = await ProcessAsTaskHelper.RunProcessAsync(
                        "/bin/sh",
                        new[] { "-c", script },
                        workingDirectory: null,
                        waitTimeForExit: timeout).ConfigureAwait(false);

                return processResults;
            }
            catch (InvalidOperationException invalidOperationException)
            {
                Console.WriteLine($"Process failed to start {invalidOperationException.Message}");
            }

            return null;
        }

    }
}
