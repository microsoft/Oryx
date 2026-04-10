// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

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

        /// <summary>
        /// Default PHP major.minor version per OS flavor, matching platforms/php/versions/*/defaultVersion.txt.
        /// </summary>
        public static readonly Dictionary<string, string> DefaultVersionPerFlavor = new Dictionary<string, string>
        {
            { "bookworm", "8.3" },
            { "bullseye", "8.0" },
            { "buster", "8.0" },
            { "focal-scm", "8.0" },
            { "noble", "8.5" },
            { "stretch", "8.0" },
        };

        /// <summary>
        /// Default PHP Composer major version per OS flavor, matching platforms/php/composer/versions/*/defaultVersion.txt.
        /// </summary>
        public static readonly Dictionary<string, string> ComposerDefaultVersionPerFlavor = new Dictionary<string, string>
        {
            { "bookworm", "2" },
            { "bullseye", "2" },
            { "buster", "2" },
            { "focal-scm", "2" },
            { "noble", "2" },
            { "stretch", "2" },
        };
    }
}
