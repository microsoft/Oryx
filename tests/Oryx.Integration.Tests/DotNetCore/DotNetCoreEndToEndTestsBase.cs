// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using System;
using System.IO;
using System.Reflection.Metadata.Ecma335;
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
        protected const string NetCoreApp31MvcApp = "NetCoreApp31.MvcApp";
        protected const string Net5MvcApp = "Net5MvcApp";
        protected const string NetCoreApp60MvcApp = "NetCore6PreviewMvcApp";
        protected const string NetCoreApp70MvcApp = "NetCore7PreviewMvcApp";
        protected const string NetCoreApp70WebApp = "NetCore7WebApp";
        protected const string NetCoreApp80MvcApp = "NetCore8PreviewMvcApp";
        protected const string DefaultWebApp = "DefaultWebApp";
        protected const string NetCoreApp21MultiProjectApp = "NetCoreApp21MultiProjectApp";
        protected const string DefaultStartupFilePath = "./run.sh";

        public DotNetCoreEndToEndTestsBase(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        protected DockerVolume CreateDefaultWebAppVolume()
        {
            return DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "DotNetCore", DefaultWebApp));
        }

        protected new DockerVolume CreateAppOutputDirVolume()
        {
            var appOutputDirPath = Directory.CreateDirectory(Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N")))
                .FullName;
            return DockerVolume.CreateMirror(appOutputDirPath);
        }
    }
}