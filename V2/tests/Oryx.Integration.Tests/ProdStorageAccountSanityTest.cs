// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.Integration.Tests
{
    [Trait("StorageAccountTests", "Prod")]
    public class ProdStorageAccountSanityTest : StorageAccountSanityTestBase
    {
        public ProdStorageAccountSanityTest(
            ITestOutputHelper output,
            TestTempDirTestFixture testTempDirTestFixture,
            RepoRootDirTestFixture repoRootDirTestFixture)
            : base(SdkStorageConstants.ProdSdkStorageBaseUrl, output, testTempDirTestFixture, repoRootDirTestFixture)
        {
        }
    }
}
