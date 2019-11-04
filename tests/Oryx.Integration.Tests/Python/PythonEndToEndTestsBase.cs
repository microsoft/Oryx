// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System.IO;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public abstract class PythonEndToEndTestsBase : PlatformEndToEndTestsBase
    {
        protected const int ContainerPort = 3000;
        protected const string DefaultStartupFilePath = "./run.sh";

        protected readonly ITestOutputHelper _output;
        protected readonly string _hostSamplesDir;
        protected readonly string _tempRootDir;

        public PythonEndToEndTestsBase(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
        {
            _output = output;
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _tempRootDir = testTempDirTestFixture.RootDirPath;
        }

        protected DockerVolume CreateAppVolume(string appName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "python", appName));
    }
}