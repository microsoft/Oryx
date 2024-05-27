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
    [Trait("StorageAccountTests", "Test")]
    public class TestStorageAccountSanityTest : StorageAccountSanityTestBase
    {
        public TestStorageAccountSanityTest(
            ITestOutputHelper output,
            TestTempDirTestFixture testTempDirTestFixture,
            RepoRootDirTestFixture repoRootDirTestFixture)
            : base(
                Environment.GetEnvironmentVariable(SdkStorageConstants.TestingSdkStorageUrlKeyName) 
                  ?? throw new InvalidOperationException($"Environment variable '{SdkStorageConstants.SdkStorageBaseUrlKeyName}' is required."),
                output, 
                testTempDirTestFixture, 
                repoRootDirTestFixture)
        {
        }
    }
}
