// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    /// <summary>
    /// Basic constructs used to run sample app builds in Docker containers.
    /// </summary>
    public abstract class SampleAppsTestBase
    {
        private readonly ITestOutputHelper _output;
        protected readonly string _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
        protected readonly DockerCli _dockerCli;

        public static EnvironmentVariable CreateAppNameEnvVar(string sampleAppName) =>
            new EnvironmentVariable(LoggingConstants.AppServiceAppNameEnvironmentVariableName, sampleAppName);

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
