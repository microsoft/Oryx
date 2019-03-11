// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System;
using System.IO;
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

        public static EnvironmentVariable CreateAppNameEnvVar(string sampleAppName) =>
            new EnvironmentVariable(LoggingConstants.AppServiceAppNameEnvironmentVariableName, sampleAppName);

        public SampleAppsTestBase(ITestOutputHelper output)
        {
            _output = output;
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
