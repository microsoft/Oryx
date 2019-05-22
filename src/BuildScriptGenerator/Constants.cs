// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class Constants
    {
        public const string OryxEnvironmentSettingNamePrefix = "ORYX_";
        public const string BuildEnvironmentFileName = "build.env";
        public const string ManifestFileName = "oryx-manifest.toml";
        public const string AppInsightsKey = "APPINSIGHTS_INSTRUMENTATIONKEY";
        public const string ZipAllOutputBuildPropertyKey = "zip_all_output";
        public const string ZipAllOutputBuildPropertyKeyDocumentation =
            "Zips entire output content and puts the file in the destination directory." +
            "Options are 'true', blank (same meaning as 'true'), and 'false'. Default is false.";

        public const string OryxGitHubUrl = "https://github.com/microsoft/Oryx";
    }
}