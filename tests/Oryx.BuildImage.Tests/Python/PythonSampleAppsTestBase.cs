// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Oryx.Tests.Common;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class PythonSampleAppsTestBase : SampleAppsTestBase
    {
        public const string PackagesDirectory = "__oryx_packages__";

        public DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "python", sampleAppName));

        public PythonSampleAppsTestBase(ITestOutputHelper output) : base(output)
        {
        }
    }
}
