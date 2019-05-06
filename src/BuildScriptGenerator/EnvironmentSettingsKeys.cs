// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class EnvironmentSettingsKeys
    {
        // Note: These two keys exist so that we do not break existing users who might still be using them
        public const string PreBuildScriptPath = "PRE_BUILD_SCRIPT_PATH";
        public const string PostBuildScriptPath = "POST_BUILD_SCRIPT_PATH";

        /// <summary>
        /// Represents an line script or a path to a file
        /// </summary>
        public const string PreBuildScript = "PRE_BUILD_SCRIPT";

        /// <summary>
        /// Represents an line script or a path to a file
        /// </summary>
        public const string PostBuildScript = "POST_BUILD_SCRIPT";

        public const string DotnetCoreDefaultVersion = "ORYX_DOTNETCORE_DEFAULT_VERSION";

        public const string DotnetCoreSupportedVersions = "DOTNETCORE_SUPPORTED_VERSIONS";

        public const string Project = "PROJECT";

        public const string DisableCollectStatic = "DISABLE_COLLECTSTATIC";
    }
}
