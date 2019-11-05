// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System.IO;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public abstract class DotNetCoreEndToEndTestsBase : PlatformEndToEndTestsBase
    {
        protected const int ContainerPort = 3000;
        protected const string NetCoreApp11WebApp = "NetCoreApp11WebApp";
        protected const string NetCoreApp21WebApp = "NetCoreApp21.WebApp";
        protected const string NetCoreApp22WebApp = "NetCoreApp22WebApp";
        protected const string NetCoreApp30WebApp = "NetCoreApp30.WebApp";
        protected const string NetCoreApp30MvcApp = "NetCoreApp30.MvcApp";
        protected const string DefaultWebApp = "DefaultWebApp";
        protected const string NetCoreApp21MultiProjectApp = "NetCoreApp21MultiProjectApp";
        protected const string DefaultStartupFilePath = "./run.sh";

        protected readonly ITestOutputHelper _output;
        protected readonly string _hostSamplesDir;
        protected readonly string _tempRootDir;

        public DotNetCoreEndToEndTestsBase(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
        {
            _output = output;
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _tempRootDir = testTempDirTestFixture.RootDirPath;
        }

        protected DockerVolume CreateDefaultWebAppVolume()
        {
            return DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "DotNetCore", DefaultWebApp));
        }
    }
}