// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// ACR-based version provider for .NET SDKs via external socket.
    /// Unlike simple platforms, .NET requires a runtime→SDK version mapping.
    /// The external host writes a JSON mapping file to the cache directory.
    /// This class owns its socket communication for version discovery.
    /// </summary>
    public class DotNetCoreExternalAcrVersionProvider : IDotNetCoreVersionProvider
    {
        private const string SocketPath = "/var/sockets/oryx-pull-sdk-image.socket";
        private const string ExternalSdksStorageDir = "/var/OryxSdks";
        private const int MaxTimeoutForSocketOperationInSeconds = 100;

        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly ILogger<DotNetCoreExternalAcrVersionProvider> logger;
        private Dictionary<string, string> versionMap;
        private string defaultRuntimeVersion;

        public DotNetCoreExternalAcrVersionProvider(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILoggerFactory loggerFactory)
        {
            this.commonOptions = commonOptions.Value;
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
            var debianFlavor = this.commonOptions.DebianFlavor ?? "bookworm";

            this.logger.LogInformation(
                "Getting .NET version info from ACR via external socket for {DebianFlavor}.",
                debianFlavor);

            // Send a socket request to fetch version info
            var request = new ExternalAcrVersionRequest
            {
                PlatformName = platformName,
                BlobName = null,
                UrlParameters = new Dictionary<string, string>
                {
                    { "source", "acr" },
                    { "action", "list-versions" },
                    { "debianFlavor", debianFlavor },
                },
            };

            var response = this.SendRequestAsync(request).GetAwaiter().GetResult();

            // External host writes a JSON mapping file:
            // {"mappings": {"8.0.5": "8.0.301", ...}, "defaultRuntimeVersion": "8.0.5"}
            var mappingFilePath = Path.Combine(
                ExternalSdksStorageDir, platformName, $"acr-dotnet-versions-{debianFlavor}.json");

            if (response && File.Exists(mappingFilePath))
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
                            "Got .NET version map from external ACR socket with {Count} entries.",
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

            // Fallback: read flat version list file
            var versionsFilePath = Path.Combine(
                ExternalSdksStorageDir, platformName, $"acr-versions-{debianFlavor}.txt");

            this.versionMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists(versionsFilePath))
            {
                var content = File.ReadAllText(versionsFilePath).Trim();
                var versions = content
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Trim())
                    .Where(v => !string.IsNullOrEmpty(v));
                foreach (var version in versions)
                {
                    this.versionMap[version] = version;
                }
            }

            // Read default version
            var defaultVersionFilePath = Path.Combine(
                ExternalSdksStorageDir, platformName, $"acr-default-version-{debianFlavor}.txt");
            if (File.Exists(defaultVersionFilePath))
            {
                this.defaultRuntimeVersion = File.ReadAllText(defaultVersionFilePath).Trim();
                if (string.IsNullOrEmpty(this.defaultRuntimeVersion))
                {
                    this.defaultRuntimeVersion = null;
                }
            }

            this.logger.LogInformation(
                "Using .NET version info from external ACR socket with {Count} entries (default: {Default}).",
                this.versionMap.Count,
                this.defaultRuntimeVersion ?? "none");
        }

        private async Task<bool> SendRequestAsync(ExternalAcrVersionRequest request)
        {
            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            try
            {
                using (var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(MaxTimeoutForSocketOperationInSeconds)))
                {
                    await socket.ConnectAsync(new UnixDomainSocketEndPoint(SocketPath), cts.Token);
                    var requestJson = JsonSerializer.Serialize(request) + "$";
                    var requestBytes = Encoding.UTF8.GetBytes(requestJson);

                    await socket.SendAsync(new ArraySegment<byte>(requestBytes), SocketFlags.None, cts.Token);
                    var buffer = new byte[4096];
                    var received = await socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), SocketFlags.None, cts.Token);
                    var responseString = Encoding.UTF8.GetString(buffer, 0, received);

                    if (!string.IsNullOrEmpty(responseString) && responseString.EqualsIgnoreCase("Success$"))
                    {
                        return true;
                    }

                    this.logger.LogError(
                        ".NET ACR version request via socket was unsuccessful. Response: {Response}",
                        responseString);
                }
            }
            catch (OperationCanceledException)
            {
                this.logger.LogError(".NET ACR version request via socket timed out.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error communicating with external ACR provider for .NET version info.");
            }

            return false;
        }

        private class ExternalAcrVersionRequest
        {
            public string PlatformName { get; set; }

            public string BlobName { get; set; }

            public IDictionary<string, string> UrlParameters { get; set; }
        }

        private class DotNetExternalAcrCatalog
        {
            public Dictionary<string, string> Mappings { get; set; }

            public string DefaultRuntimeVersion { get; set; }
        }
    }
}
