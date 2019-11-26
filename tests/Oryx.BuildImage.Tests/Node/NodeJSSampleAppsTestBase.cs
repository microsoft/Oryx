// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class NodeJSSampleAppsTestBase : SampleAppsTestBase, IClassFixture<TestTempDirTestFixture>
    {
        public static readonly string SampleAppName = "webfrontend";
        
        protected readonly string _tempRootDir;

        public DockerVolume CreateWebFrontEndVolume() => DockerVolume.CreateMirror(
            Path.Combine(_hostSamplesDir, "nodejs", SampleAppName));

        public NodeJSSampleAppsTestBase(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture) :
            base(output, new DockerCli(new EnvironmentVariable[]
            {
                new EnvironmentVariable(ExtVarNames.AppServiceAppNameEnvVarName, SampleAppName)
            }))
        {
            _tempRootDir = testTempDirTestFixture.RootDirPath;
        }
    }
}