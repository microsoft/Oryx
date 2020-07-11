// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator.Common;
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
        protected readonly ImageTestHelper _imageHelper;
        protected readonly ImageTestHelper _restrictedPermissionsImageHelper;

        public static EnvironmentVariable CreateAppNameEnvVar(string sampleAppName) =>
            new EnvironmentVariable(ExtVarNames.AppServiceAppNameEnvVarName, sampleAppName);

        public SampleAppsTestBase(ITestOutputHelper output, DockerCli dockerCli = null)
        {
            _output = output;
            _dockerCli = dockerCli ?? new DockerCli();
            _imageHelper = new ImageTestHelper(output);
            _restrictedPermissionsImageHelper = ImageTestHelper.WithRestrictedPermissions(output);
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
