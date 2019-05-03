// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests.LocalDockerTests
{
    [Trait("Category", "END")]
    public class NotifyEndOfIntegrationTests : PlatformEndToEndTestsBase
    {
        private readonly ITestOutputHelper _output;
        private readonly string _hostSamplesDir;
        private readonly string _tempRootDir;

        public NotifyEndOfIntegrationTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
        {
            _output = output;
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _tempRootDir = testTempDirTestFixture.RootDirPath;
        }
    }
}