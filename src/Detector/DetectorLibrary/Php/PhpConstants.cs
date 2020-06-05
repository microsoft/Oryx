// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector.Php
{
    public class PhpConstants
    {
        public const string PlatformName = "php";
        public const string PhpFileNamePattern = "*.php";
        public const string ComposerFileName = "composer.json";
        public const string PhpRuntimeVersionEnvVarName = "PHP_VERSION";
        public const string DefaultPhpRuntimeVersion = "7.3.15";
        public const string InstalledPhpVersionsDir = "/opt/php/"; // TODO: consolidate with Dockerfile to yaml?
    }
}
