// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.Integration.Tests
{
    [Trait("StorageAccountTests", "SanityTests")]
    public class ProdStorageAccountSanityTest : StorageAccountSanityTestBase
    {
        public ProdStorageAccountSanityTest(
            ITestOutputHelper output,
            TestTempDirTestFixture testTempDirTestFixture,
            RepoRootDirTestFixture repoRootDirTestFixture)
            : base(Environment.GetEnvironmentVariable(SdkStorageConstants.SdkStorageBaseUrlKeyName)
                  ?? throw new InvalidOperationException($"Environment variable '{SdkStorageConstants.SdkStorageBaseUrlKeyName}' is required."),
                  output, 
                  testTempDirTestFixture, 
                  repoRootDirTestFixture)
        {
        }
    }
}
