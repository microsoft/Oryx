// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class EnvironmentSettingsKeys
    {
        // Note: The following two constants exist so that we do not break
        // existing users who might still be using them
        public const string PreBuildScriptPath = "PRE_BUILD_SCRIPT_PATH";
        public const string PostBuildScriptPath = "POST_BUILD_SCRIPT_PATH";

        /// <summary>
        /// Represents an line script or a path to a file.
        /// </summary>
        public const string PreBuildCommand = "PRE_BUILD_COMMAND";

        /// <summary>
        /// Represents an line script or a path to a file.
        /// </summary>
        public const string PostBuildCommand = "POST_BUILD_COMMAND";

        public const string DotNetVersion = "DOTNET_VERSION";

        public const string Project = "PROJECT";

        public const string DisableCollectStatic = "DISABLE_COLLECTSTATIC";

        /// <summary>
        /// Represents the 'Configuration' switch of a build, for example: dotnet build --configuration Release.
        /// </summary>
        public const string MSBuildConfiguration = "MSBUILD_CONFIGURATION";
    }
}
