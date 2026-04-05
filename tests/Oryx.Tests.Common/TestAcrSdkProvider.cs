// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestAcrSdkProvider : IAcrSdkProvider
    {
        public Task<bool> RequestSdkFromAcrAsync(string platformName, string version, string debianFlavor, string runtimeVersion = null)
        {
            return Task.FromResult(false);
        }
    }
}
