// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonSdkStorageVersionProvider : SdkStorageVersionProviderBase, IPythonVersionProvider
    {
        public PythonSdkStorageVersionProvider(IEnvironment environment) : base(environment)
        {
        }

        // To enable unit testing
        public virtual PlatformVersionInfo GetVersionInfo()
        {
            return GetAvailableVersionsFromStorage(
                platformName: ToolNameConstants.PythonName,
                versionMetadataElementName: "Version");
        }
    }
}