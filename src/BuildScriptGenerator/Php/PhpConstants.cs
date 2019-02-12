// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpConstants
    {
        internal const string PhpName = "php";
        internal const string ComposerFileName = "composer.json";
        internal const string DefaultPhpRuntimeVersion = "???";
        internal const string InstalledPhpVersionsDir = "/opt/php/"; // TODO: consolidate with Dockerfile to yaml?
    }
}
