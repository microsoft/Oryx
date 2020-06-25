// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using System.IO;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class HugoSampleAppsTestBase : SampleAppsTestBase
    {
        public static readonly string SampleAppName = "hugo-sample";

        public DockerVolume CreateSampleAppVolume() => DockerVolume.CreateMirror(
            Path.Combine(_hostSamplesDir, "hugo", SampleAppName));

        public DockerVolume CreateSampleAppVolume(string appName) => DockerVolume.CreateMirror(
            Path.Combine(_hostSamplesDir, "hugo", appName));

        public HugoSampleAppsTestBase(ITestOutputHelper output) :
            base(output, new DockerCli(new EnvironmentVariable[]
            {
                new EnvironmentVariable(ExtVarNames.AppServiceAppNameEnvVarName, SampleAppName)
            }))
        {
        }
    }
}