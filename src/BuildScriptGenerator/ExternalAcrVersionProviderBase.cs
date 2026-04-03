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
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Base class for version providers that discover available SDK versions from ACR
    /// via a Unix socket to the external host.
    /// Each per-platform subclass calls <see cref="GetAvailableVersionsFromExternalAcr"/>
    /// with its platform name.
    /// </summary>
    /// <remarks>
    /// Flow: Oryx → Unix socket → external host → ACR tag listing.
    /// This class owns the socket communication for version discovery.
    /// SDK pulling is handled separately by <see cref="ExternalAcrSdkProvider"/>.
    /// </remarks>
    public class ExternalAcrVersionProviderBase
    {
        private const string SocketPath = "/var/sockets/oryx-pull-sdk.socket";
        private const string ExternalSdksStorageDir = "/var/OryxSdks";
        private const int MaxTimeoutForSocketOperationInSeconds = 120;

        private readonly ILogger logger;
        private readonly BuildScriptGeneratorOptions commonOptions;

        public ExternalAcrVersionProviderBase(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            ILoggerFactory loggerFactory)
        {
            this.commonOptions = commonOptions.Value;
            this.logger = loggerFactory.CreateLogger(this.GetType());
        }

        /// <summary>
        /// Gets the list of available versions and default version for <paramref name="platformName"/>
        /// from ACR via the external host socket.
        /// </summary>
        /// Update this method to read the version list from a file written by the external host after it queries ACR,
        protected PlatformVersionInfo GetAvailableVersionsFromExternalAcr(string platformName)
        {
            var debianFlavor = this.commonOptions.DebianFlavor ?? "bookworm";

            this.logger.LogInformation(
                "Getting available versions for platform {PlatformName} from ACR via external socket ({DebianFlavor}).",
                platformName,
                debianFlavor);

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

            // External host writes the version list to this file
            var versionsFilePath = Path.Combine(
                ExternalSdksStorageDir, platformName, $"acr-versions-{debianFlavor}.txt");

            var supportedVersions = new List<string>();
            if (response && File.Exists(versionsFilePath))
            {
                var content = File.ReadAllText(versionsFilePath).Trim();
                supportedVersions = content
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Trim())
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();
            }
            else
            {
                this.logger.LogWarning(
                    "Failed to get ACR version list via external socket for {PlatformName}.",
                    platformName);
            }

            // Read default version from a separate file if the external host wrote one
            string defaultVersion = null;
            var defaultVersionFilePath = Path.Combine(
                ExternalSdksStorageDir, platformName, $"acr-default-version-{debianFlavor}.txt");
            if (File.Exists(defaultVersionFilePath))
            {
                defaultVersion = File.ReadAllText(defaultVersionFilePath).Trim();
                if (string.IsNullOrEmpty(defaultVersion))
                {
                    defaultVersion = null;
                }
            }

            this.logger.LogInformation(
                "Found {Count} versions for {PlatformName} from ACR via external socket (default: {Default}).",
                supportedVersions.Count,
                platformName,
                defaultVersion ?? "none");

            return PlatformVersionInfo.CreateAvailableOnAcr(supportedVersions, defaultVersion);
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
                        "ACR version request via socket was unsuccessful. Response: {Response}",
                        responseString);
                }
            }
            catch (OperationCanceledException)
            {
                this.logger.LogError("ACR version request via socket timed out.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error communicating with external ACR provider for version info.");
            }

            return false;
        }

        private class ExternalAcrVersionRequest
        {
            public string PlatformName { get; set; }

            public string BlobName { get; set; }

            public IDictionary<string, string> UrlParameters { get; set; }
        }
    }
}
