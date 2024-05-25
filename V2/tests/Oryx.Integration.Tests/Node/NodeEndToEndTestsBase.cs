// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Oryx.Tests.Common;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public class NodeEndToEndTestsBase : PlatformEndToEndTestsBase
    {
        public readonly int ContainerPort = 3000;
        public readonly int DebugPort = 9000;
        public readonly string DefaultStartupFilePath = "./run.sh";

        public NodeEndToEndTestsBase(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        protected DockerVolume CreateAppVolume(string appName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "nodejs", appName));
    }
}