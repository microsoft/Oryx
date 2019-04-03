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

        public const string ZipAllOutputBuildPropertyKey = "zip_all_output";
        public const string ZippedOutputFileName = "oryx_output.tar.gz";
    }
}