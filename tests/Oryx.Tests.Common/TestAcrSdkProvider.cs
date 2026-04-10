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
        private readonly bool _returnValue;

        public TestAcrSdkProvider(bool returnValue = false)
        {
            _returnValue = returnValue;
        }

        public bool RequestSdkFromAcrAsyncCalled { get; private set; }

        public string LastRequestedPlatformName { get; private set; }

        public string LastRequestedVersion { get; private set; }

        public string LastRequestedDebianFlavor { get; private set; }

        public string LastRequestedRuntimeVersion { get; private set; }

        public Task<bool> RequestSdkFromAcrAsync(string platformName, string version, string debianFlavor, string runtimeVersion = null)
        {
            RequestSdkFromAcrAsyncCalled = true;
            LastRequestedPlatformName = platformName;
            LastRequestedVersion = version;
            LastRequestedDebianFlavor = debianFlavor;
            LastRequestedRuntimeVersion = runtimeVersion;
            return Task.FromResult(_returnValue);
        }
    }
}
