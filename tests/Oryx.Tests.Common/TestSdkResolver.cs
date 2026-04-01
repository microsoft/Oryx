// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.Tests.Common
{
    public class TestSdkResolver : ISdkResolver
    {
        private readonly bool _sdkFetchResult;

        public TestSdkResolver(bool sdkFetchResult = false)
        {
            _sdkFetchResult = sdkFetchResult;
        }

        public bool TryFetchSdk(string platformName, string version, string debianFlavor)
        {
            return _sdkFetchResult;
        }
    }
}
