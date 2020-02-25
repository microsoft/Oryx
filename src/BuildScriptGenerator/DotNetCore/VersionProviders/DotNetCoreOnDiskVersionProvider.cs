// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class DotNetCoreOnDiskVersionProvider : IDotNetCoreVersionProvider
    {
        public string GetDefaultRuntimeVersion()
        {
            return DotNetCoreRunTimeVersions.NetCoreApp31;
        }

        public Dictionary<string, string> GetSupportedVersions()
        {
            var versionMap = new Dictionary<string, string>();
            var installedRuntimeVersions = VersionProviderHelper.GetVersionsFromDirectory(
                        DotNetCoreConstants.InstalledDotNetCoreRuntimeVersionsDir);
            foreach (var runtimeVersion in installedRuntimeVersions)
            {
                var runtimeDir = Path.Combine(
                    DotNetCoreConstants.InstalledDotNetCoreRuntimeVersionsDir,
                    runtimeVersion);
                var sdkVersionFile = Path.Combine(runtimeDir, "sdkVersion.txt");
                if (!File.Exists(sdkVersionFile))
                {
                    throw new InvalidOperationException($"Could not find file '{sdkVersionFile}'.");
                }

                var sdkVersion = File.ReadAllText(sdkVersionFile);
                if (string.IsNullOrEmpty(sdkVersion))
                {
                    throw new InvalidOperationException("Sdk version cannot be empty.");
                }

                versionMap[runtimeVersion] = sdkVersion.Trim();
            }

            return versionMap;
        }
    }
}
