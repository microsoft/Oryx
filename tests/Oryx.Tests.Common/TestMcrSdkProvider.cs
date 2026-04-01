// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestMcrSdkProvider : IMcrSdkProvider
    {
        public Task<bool> PullSdkAsync(string platformName, string version, string debianFlavor)
        {
            return Task.FromResult(false);
        }
    }
}
