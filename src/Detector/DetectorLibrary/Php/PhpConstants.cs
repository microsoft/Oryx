// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector.Php
{
    internal class PhpConstants
    {
        internal const string PlatformName = "php";
        internal const string PhpFileNamePattern = "*.php";
        internal const string ComposerFileName = "composer.json";
        internal const string PhpRuntimeVersionEnvVarName = "PHP_VERSION";
        internal const string DefaultPhpRuntimeVersion = "7.3.15";
        internal const string InstalledPhpVersionsDir = "/opt/php/"; // TODO: consolidate with Dockerfile to yaml?
    }
}
