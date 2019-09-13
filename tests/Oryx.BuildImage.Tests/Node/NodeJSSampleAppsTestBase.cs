// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System.IO;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class NodeJSSampleAppsTestBase : SampleAppsTestBase
    {
        public static readonly string SampleAppName = "webfrontend";

        public DockerVolume CreateWebFrontEndVolume() => DockerVolume.CreateMirror(
            Path.Combine(_hostSamplesDir, "node", SampleAppName));

        public NodeJSSampleAppsTestBase(ITestOutputHelper output) :
            base(output, new DockerCli(new EnvironmentVariable[]
            {
                new EnvironmentVariable(ExtVarNames.AppServiceAppNameEnvVarName, SampleAppName)
            }))
        {
        }
    }
}