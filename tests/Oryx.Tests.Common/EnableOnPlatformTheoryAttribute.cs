// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Runtime.InteropServices;
using Xunit;

namespace Microsoft.Oryx.Tests.Common
{
    public class EnableOnPlatformTheoryAttribute : TheoryAttribute
    {
        private readonly OSPlatform _platform;

        public EnableOnPlatformTheoryAttribute(string platform)
        {
            _platform = OSPlatform.Create(platform);

            if (!RuntimeInformation.IsOSPlatform(_platform))
            {
                Skip = $"This test can only run on platform '{_platform}'.";
            }
        }
    }
}
