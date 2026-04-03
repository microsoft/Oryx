// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestExternalAcrSdkProvider : IExternalAcrSdkProvider
    {
        public Task<IList<string>> GetVersionsAsync(string platformName, string debianFlavor)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<string> GetDefaultVersionAsync(string platformName, string debianFlavor)
        {
            return Task.FromResult<string>(null);
        }

        public Task<bool> RequestSdkAsync(string platformName, string version, string debianFlavor)
        {
            return Task.FromResult(false);
        }
    }
}
