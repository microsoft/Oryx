// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// ACR-based version provider for .NET SDKs via external socket provider.
    /// Unlike simple platforms, .NET requires a runtime→SDK version mapping.
    /// The external host writes a JSON file with the mapping to the cache directory.
    /// Parallel to <see cref="DotNetCoreExternalVersionProvider"/> (blob) and
    /// <see cref="DotNetCoreAcrVersionProvider"/> (direct OCI).
    /// </summary>
    public class DotNetCoreExternalAcrVersionProvider : IDotNetCoreVersionProvider
    {
        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly IExternalAcrSdkProvider externalAcrProvider;
        private readonly ILogger<DotNetCoreExternalAcrVersionProvider> logger;
        private Dictionary<string, string> versionMap;
        private string defaultRuntimeVersion;

        public DotNetCoreExternalAcrVersionProvider(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IExternalAcrSdkProvider externalAcrSdkProvider,
            ILoggerFactory loggerFactory)
        {
            this.commonOptions = commonOptions.Value;
            this.externalAcrProvider = externalAcrSdkProvider;
            this.logger = loggerFactory.CreateLogger<DotNetCoreExternalAcrVersionProvider>();
        }

        public string GetDefaultRuntimeVersion()
        {
            this.EnsureVersionInfo();
            return this.defaultRuntimeVersion;
        }

        public Dictionary<string, string> GetSupportedVersions()
        {
            this.EnsureVersionInfo();
            return this.versionMap;
        }

        private void EnsureVersionInfo()
        {
            if (this.versionMap != null)
            {
                return;
            }

            var platformName = DotNetCoreConstants.PlatformName;
            var debianFlavor = this.commonOptions.DebianFlavor;

            this.logger.LogInformation(
                "Getting .NET version info from ACR via external provider for {DebianFlavor}.",
                debianFlavor);

            // External host writes a JSON mapping file: {"mappings": {"8.0.5": "8.0.301", ...}, "defaultRuntimeVersion": "8.0.5"}
            var mappingFileName = $"acr-dotnet-versions-{debianFlavor}.json";
            var mappingFilePath = Path.Combine(
                ExternalAcrSdkProvider.ExternalSdksStorageDir,
                platformName,
                mappingFileName);

            // Request the mapping file from the external provider
            var versions = this.externalAcrProvider
                .GetVersionsAsync(platformName, debianFlavor)
                .GetAwaiter()
                .GetResult();

            // Try to read the JSON mapping file that the external host writes
            if (File.Exists(mappingFilePath))
            {
                try
                {
                    var json = File.ReadAllText(mappingFilePath);
                    var catalog = JsonSerializer.Deserialize<DotNetExternalAcrCatalog>(json);
                    if (catalog?.Mappings != null)
                    {
                        this.versionMap = new Dictionary<string, string>(
                            catalog.Mappings, StringComparer.OrdinalIgnoreCase);
                        this.defaultRuntimeVersion = catalog.DefaultRuntimeVersion;
                        this.logger.LogInformation(
                            "Got .NET version map from external ACR provider with {Count} entries.",
                            this.versionMap.Count);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning(
                        ex,
                        "Failed to parse .NET version mapping from {FilePath}. Falling back to flat version list.",
                        mappingFilePath);
                }
            }

            // Fallback: use the flat version list (1:1 mapping, version = SDK version)
            this.versionMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (versions != null)
            {
                foreach (var version in versions)
                {
                    this.versionMap[version] = version;
                }
            }

            this.defaultRuntimeVersion = this.externalAcrProvider
                .GetDefaultVersionAsync(platformName, debianFlavor)
                .GetAwaiter()
                .GetResult();

            this.logger.LogInformation(
                "Using flat version list from external ACR provider with {Count} entries for .NET (default: {Default}).",
                this.versionMap.Count,
                this.defaultRuntimeVersion ?? "none");
        }

        private class DotNetExternalAcrCatalog
        {
            public Dictionary<string, string> Mappings { get; set; }

            public string DefaultRuntimeVersion { get; set; }
        }
    }
}
