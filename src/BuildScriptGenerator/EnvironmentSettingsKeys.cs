// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class EnvironmentSettingsKeys
    {
        public const string PreBuildScriptPath = "PRE_BUILD_SCRIPT_PATH";

        public const string PostBuildScriptPath = "POST_BUILD_SCRIPT_PATH";

        public const string DotnetCoreDefaultVersion = "ORYX_DOTNETCORE_DEFAULT_VERSION";

        public const string DotnetCoreSupportedVersions = "DOTNETCORE_SUPPORTED_VERSIONS";

        public const string Project = "PROJECT";
    }
}
