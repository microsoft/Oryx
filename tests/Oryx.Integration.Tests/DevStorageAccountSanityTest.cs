﻿// --------------------------------------------------------------------------------------------
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
    [Trait("StorageAccountTests", "Dev")]
    public class DevStorageAccountSanityTest : StorageAccountSanityTestBase
    {
        public DevStorageAccountSanityTest(
            ITestOutputHelper output,
            TestTempDirTestFixture testTempDirTestFixture,
            RepoRootDirTestFixture repoRootDirTestFixture)
            : base(
                Environment.GetEnvironmentVariable(SdkStorageConstants.TestingSdkStorageUrlKeyName) ?? SdkStorageConstants.DevSdkStorageBaseUrl,
                output, 
                testTempDirTestFixture, 
                repoRootDirTestFixture,
                string.Empty)
        {
        }
    }
}
