// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Ruby
{
    internal interface IRubyVersionProvider
    {
        PlatformVersionInfo GetVersionInfo();
    }
}