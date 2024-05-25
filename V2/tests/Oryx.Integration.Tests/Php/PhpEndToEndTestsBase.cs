// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.Tests.Common;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public abstract class PhpEndToEndTestsBase : PlatformEndToEndTestsBase
    {
        public readonly int ContainerPort = 8080;
        public readonly string RunScriptPath = "/tmp/startup.sh";
        public readonly IList<string> _downloadedPaths = new List<string>();

        public PhpEndToEndTestsBase(ITestOutputHelper output, TestTempDirTestFixture fixture) : base(output, fixture)
        {
        }
    }
}
