// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal interface IPhpComposerVersionProvider
    {
        PlatformVersionInfo GetVersionInfo();
    }
}