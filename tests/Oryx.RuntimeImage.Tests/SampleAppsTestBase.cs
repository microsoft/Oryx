// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    /// <summary>
    /// Basic constructs used to run sample app builds in Docker containers.
    /// </summary>
    public abstract class SampleAppsTestBase
    {
        protected readonly ITestOutputHelper _output;
        protected readonly int ContainerPort = 8080;
        protected readonly string _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
        protected readonly DockerCli _dockerCli;
        protected readonly HttpClient _httpClient = new HttpClient();

        // The following method is used to avoid following exception from HttpClient when trying to read a response:
        // '"utf-8"' is not a supported encoding name. For information on defining a custom encoding,
        // see the documentation for the Encoding.RegisterProvider method.
        protected async Task<string> GetResponseDataAsync(string url)
        {
            var bytes = await _httpClient.GetByteArrayAsync(url);
            return Encoding.UTF8.GetString(bytes);
        }

        public static EnvironmentVariable CreateAppNameEnvVar(string sampleAppName) =>
            new EnvironmentVariable(ExtVarNames.AppServiceAppNameEnvVarName, sampleAppName);

        public SampleAppsTestBase(ITestOutputHelper output, DockerCli dockerCli = null)
        {
            _output = output;
            _dockerCli = dockerCli ?? new DockerCli();
        }

        protected void RunAsserts(Action action, string message)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                _output.WriteLine(message);
                throw;
            }
        }
    }
}
