// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestExternalAcrSdkProvider : IExternalAcrSdkProvider
    {
        private readonly bool _returnValue;

        public TestExternalAcrSdkProvider(bool returnValue = false)
        {
            _returnValue = returnValue;
        }

        public bool RequestSdkAsyncCalled { get; private set; }

        public string LastRequestedPlatformName { get; private set; }

        public string LastRequestedVersion { get; private set; }

        public string LastRequestedDebianFlavor { get; private set; }

        public Task<bool> RequestSdkAsync(string platformName, string version, string debianFlavor)
        {
            RequestSdkAsyncCalled = true;
            LastRequestedPlatformName = platformName;
            LastRequestedVersion = version;
            LastRequestedDebianFlavor = debianFlavor;
            return Task.FromResult(_returnValue);
        }
    }
}
