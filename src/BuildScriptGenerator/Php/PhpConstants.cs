// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpConstants
    {
        internal const string PhpName = "php";
        internal const string ComposerName = "composer";
        internal const string PhpFileNamePattern = "*.php";
        internal const string ComposerFileName = "composer.json";
        internal const string SupportedVersionsEnvVarName = "ORYX_PHP_SUPPORTED_VERSIONS";
        internal const string DefaultPhpRuntimeVersionEnvVarName = "ORYX_PHP_DEFAULT_VERSION";
        internal const string DefaultPhpRuntimeVersion = Common.PhpVersions.Php73Version;
        internal const string InstalledPhpVersionsDir = "/opt/php/"; // TODO: consolidate with Dockerfile to yaml?
    }
}
