// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Microsoft.Oryx.Automation
{
    public class Constants
    {
        public const string OryxSdkStorageBaseUrl = "https://oryx-cdn.microsoft.io";
        public const string OryxSdkStagingStorageBaseUrl = "https://oryxsdksstaging.blob.core.windows.net";
        public const string OryxSdkStorageBaseUrlEnvVar = "ORYX_SDK_STORAGE_BASE_URL";
        public const string VersionsToBuildTxtFileName = "versionsToBuild.txt";
        public const string ConstantsYaml = "constants.yaml";
        public static readonly HashSet<string> DebianFlavors = new HashSet<string>()
        { "bookworm", "bullseye", "buster", "focal-scm", "stretch" };
    }
}
