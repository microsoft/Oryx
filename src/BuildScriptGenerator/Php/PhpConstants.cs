// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    public static class PhpConstants
    {
        public const string PlatformName = "php";
        public const string PhpFileNamePattern = "*.php";
        public const string PhpComposerName = "php-composer";
        public const string ComposerFileName = "composer.json";
        public const string PhpRuntimeVersionEnvVarName = "PHP_VERSION";
        public const string DefaultPhpRuntimeVersion = Common.PhpVersions.Php73Version;
        public const string InstalledPhpVersionsDir = "/opt/php/"; // TODO: consolidate with Dockerfile to yaml?
        public const string InstalledPhpComposerVersionDir = "/opt/php-composer/";
    }
}
