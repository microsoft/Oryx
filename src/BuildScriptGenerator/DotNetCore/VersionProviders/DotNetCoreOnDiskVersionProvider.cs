// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class DotNetCoreOnDiskVersionProvider : IDotNetCoreVersionProvider
    {
        private readonly ILogger<DotNetCoreOnDiskVersionProvider> _logger;

        public DotNetCoreOnDiskVersionProvider(ILogger<DotNetCoreOnDiskVersionProvider> logger)
        {
            _logger = logger;
        }

        public string GetDefaultRuntimeVersion()
        {
            var defaultRuntimeVersion = DotNetCoreRunTimeVersions.NetCoreApp31;

            _logger.LogDebug("Default runtime version is {defaultRuntimeVersion}", defaultRuntimeVersion);

            return defaultRuntimeVersion;
        }

        public Dictionary<string, string> GetSupportedVersions()
        {
            var versionMap = new Dictionary<string, string>();

            _logger.LogDebug(
                "Getting list of supported runtime and their sdk versions from {installationDir}",
                DotNetCoreConstants.InstalledDotNetCoreRuntimeVersionsDir);

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
