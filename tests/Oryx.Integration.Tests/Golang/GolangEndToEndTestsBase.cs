// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System;
using System.IO;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public abstract class GolangEndToEndTestsBase : PlatformEndToEndTestsBase
    {
        protected const int ContainerPort = 3000;
        protected const string GolangHelloWorldWebApp = "hello-world";
        protected const string DefaultStartupFilePath = "./run.sh";

        public GolangEndToEndTestsBase(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        protected DockerVolume CreateDefaultWebAppVolume()
        {
            return DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "Golang", GolangHelloWorldWebApp));
        }

        protected new DockerVolume CreateAppOutputDirVolume()
        {
            var appOutputDirPath = Directory.CreateDirectory(Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N")))
                .FullName;
            return DockerVolume.CreateMirror(appOutputDirPath);
        }
    }
}