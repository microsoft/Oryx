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

        public (StatusUrl statusUrl, string manifestPath, string logFilePath) GetStatusUrls(HttpRequest requestContext, BuildServerRequests requestBody = null)
        {
            var result = new StatusUrl();
            var host = requestContext.Host.Value;
            var scheme = requestContext.Scheme;
            var outDir = string.Empty;
            var logFilePath = string.Empty;
            var manifestFilePath = "<manifest file full path>";

            if (requestBody != null
                && requestBody.Destination != null
                && requestBody.LogFile != null)
            {
                outDir = requestBody.Destination;
                logFilePath = requestBody.LogFile;
                manifestFilePath = Path.Join(outDir, FilePaths.BuildManifestFileName);
            }

            string buildUrlQueryParam = $"?manifestfilefullpath='{manifestFilePath}'&logfilefullpath='{logFilePath}'";
            string buildUrl = string.Concat(scheme, "://", host, "/build/", "CheckBuildStatus", buildUrlQueryParam);
            string serverUrl = string.Concat(scheme, "://", host, "/build/", "CheckServerStatus");

            result.BuildStatusCheckUrl = buildUrl;
            result.ServerStatusCheckUrl = serverUrl;

            return (result, manifestFilePath, logFilePath);
        }

        public StatusUrl GetBuildStatusUrl(HttpRequest requestContext, string outDir = null)
        {
            var result = new StatusUrl();
            var buildUrlQueryParam = string.Empty;
            var host = requestContext.Host.Value;
            var scheme = requestContext.Scheme;
            var buildUrl = string.Empty;
            
            if (outDir != null)
            {
                buildUrlQueryParam = $"?outDir='{outDir}'";
            }

            buildUrl = string.Concat(scheme, "://", host, "/build/", "CheckBuildProcessStatus", buildUrlQueryParam);

            result.BuildStatusCheckUrl = buildUrl;

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
