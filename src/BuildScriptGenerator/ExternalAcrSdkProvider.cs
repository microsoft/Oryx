// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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
    /// External ACR-based SDK provider that fetches SDK tarballs from OCI images.
    /// SDK images are built as <c>FROM scratch; COPY sdk.tar.gz /</c>, so each image
    /// contains exactly one layer — the SDK tarball itself.
    /// </summary>
    /// <remarks>
    /// Two pull strategies, tried in order:
    /// <list type="number">
    /// <item>
    /// <b>Unix socket (LWASv2)</b> — available inside App Service. Sends a request to
    /// LWASv2's OryxProxy with <c>source=acr</c>; LWASv2 pulls the image and extracts
    /// the tarball to <c>/var/OryxSdks</c>.
    /// </item>
    /// <item>
    /// <b>Direct OCI pull</b> — fallback when the socket is unavailable (CLI builds,
    /// local dev). Uses <see cref="OciRegistryClient"/> to fetch the manifest, extract
    /// the single layer digest, download the blob, and verify SHA256.
    /// </item>
    /// </list>
    /// </remarks>
    public class ExternalAcrSdkProvider : IExternalAcrSdkProvider
    {
        /// <summary>
        /// The directory where SDKs are cached by the external provider (same as blob-based).
        /// </summary>
        public const string ExternalSdksStorageDir = "/var/OryxSdks";

        private const string SocketPath = "/var/sockets/oryx-pull-sdk.socket";
        private const int MaxTimeoutForSocketOperationInSeconds = 120;

        private readonly ILogger<ExternalAcrSdkProvider> logger;
        private readonly IStandardOutputWriter outputWriter;
        private readonly BuildScriptGeneratorOptions options;
        private readonly OciRegistryClient ociClient;

        public ExternalAcrSdkProvider(
            IStandardOutputWriter outputWriter,
            ILogger<ExternalAcrSdkProvider> logger,
            IOptions<BuildScriptGeneratorOptions> options,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
        {
            this.logger = logger;
            this.outputWriter = outputWriter;
            this.options = options.Value;

            var registryUrl = string.IsNullOrEmpty(this.options.OryxAcrSdkRegistryUrl)
                ? SdkStorageConstants.DefaultAcrSdkRegistryUrl
                : this.options.OryxAcrSdkRegistryUrl;

            this.ociClient = new OciRegistryClient(registryUrl, httpClientFactory, loggerFactory);
        }

        /// <inheritdoc/>
        public async Task<bool> RequestSdkFromAcrAsync(string platformName, string version, string debianFlavor)
        {
            if (string.IsNullOrEmpty(platformName))
            {
                throw new ArgumentException("Platform name cannot be null or empty.", nameof(platformName));
            }

            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException("Version cannot be null or empty.", nameof(version));
            }

            if (string.IsNullOrEmpty(debianFlavor))
            {
                debianFlavor = this.options.DebianFlavor ?? "bookworm";
            }

            var blobName = $"{platformName}-{debianFlavor}-{version}.tar.gz";
            var expectedFilePath = Path.Combine(ExternalSdksStorageDir, platformName, blobName);

            this.logger.LogInformation(
                "Requesting SDK from ACR: platform={PlatformName}, version={Version}, debianFlavor={DebianFlavor}",
                platformName,
                version,
                debianFlavor);
            this.outputWriter.WriteLine(
                $"Requesting SDK from ACR: {platformName} {version} ({debianFlavor})");

            // Check if the file is already cached locally
            if (File.Exists(expectedFilePath))
            {
                this.logger.LogInformation(
                    "SDK already cached locally at {FilePath}, skipping ACR pull.",
                    expectedFilePath);
                this.outputWriter.WriteLine($"SDK already cached locally at {expectedFilePath}");
                return true;
            }

            // Strategy 1: Try Unix socket (LWASv2) if the socket exists on disk
            if (File.Exists(SocketPath))
            {
                var socketResult = await this.TryPullViaSocketAsync(platformName, version, debianFlavor, blobName, expectedFilePath);
                if (socketResult)
                {
                    return true;
                }

                this.logger.LogWarning(
                    "LWASv2 socket pull failed for {PlatformName} {Version}. Falling back to direct OCI pull.",
                    platformName,
                    version);
            }
            else
            {
                this.logger.LogDebug(
                    "LWASv2 socket not found at {SocketPath}. Using direct OCI pull.",
                    SocketPath);
            }

            // Strategy 2: Direct OCI pull — fetch manifest, get layer digest, download blob
            return await this.TryPullDirectFromAcrAsync(platformName, version, debianFlavor, expectedFilePath);
        }

        /// <summary>
        /// Pulls the SDK via LWASv2 Unix socket.
        /// </summary>
        private async Task<bool> TryPullViaSocketAsync(
            string platformName,
            string version,
            string debianFlavor,
            string blobName,
            string expectedFilePath)
        {
            try
            {
                var request = new AcrSdkProviderRequest
                {
                    PlatformName = platformName,
                    BlobName = blobName,
                    UrlParameters = new Dictionary<string, string>
                    {
                        { "source", "acr" },
                        { "version", version },
                        { "debianFlavor", debianFlavor },
                    },
                };

                var response = await this.SendSocketRequestAsync(request);

                if (response && File.Exists(expectedFilePath))
                {
                    this.logger.LogInformation(
                        "Successfully pulled SDK from ACR via LWASv2: {PlatformName} {Version}",
                        platformName,
                        version);
                    this.outputWriter.WriteLine(
                        $"Successfully pulled SDK from ACR via LWASv2: {platformName} {version}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Error requesting SDK from ACR via LWASv2: {PlatformName} {Version}",
                    platformName,
                    version);
            }

            return false;
        }

        /// <summary>
        /// Pulls the SDK directly from the ACR registry using the OCI Distribution API.
        /// SDK images are FROM scratch with a single layer that IS the tarball:
        /// <code>
        /// FROM scratch
        /// COPY sdk.tar.gz /
        /// </code>
        /// Flow: GET manifest → extract single layer digest → GET blob → verify SHA256 → save to cache.
        /// </summary>
        private async Task<bool> TryPullDirectFromAcrAsync(
            string platformName,
            string version,
            string debianFlavor,
            string expectedFilePath)
        {
            try
            {
                var repository = $"{SdkStorageConstants.AcrSdkRepositoryPrefix}/{platformName}";
                var tag = $"{debianFlavor}-{version}";

                this.logger.LogInformation(
                    "Pulling SDK directly from ACR via OCI API: {Repository}:{Tag}",
                    repository,
                    tag);
                this.outputWriter.WriteLine(
                    $"Pulling SDK directly from ACR: {repository}:{tag}");

                var success = await this.ociClient.PullSdkAsync(repository, tag, expectedFilePath);

                if (success)
                {
                    this.logger.LogInformation(
                        "Successfully pulled SDK directly from ACR: {PlatformName} {Version}, saved to {FilePath}",
                        platformName,
                        version,
                        expectedFilePath);
                    this.outputWriter.WriteLine(
                        $"Successfully pulled SDK from ACR: {platformName} {version}");
                    return true;
                }

                this.logger.LogWarning(
                    "Direct OCI pull did not succeed for {PlatformName} {Version}.",
                    platformName,
                    version);
                this.outputWriter.WriteLine(
                    $"Failed to pull SDK from ACR: {platformName} {version}");
                return false;
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Error pulling SDK directly from ACR: {PlatformName} {Version}",
                    platformName,
                    version);
                this.outputWriter.WriteLine(
                    $"Error pulling SDK from ACR: {platformName} {version}: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SendSocketRequestAsync(AcrSdkProviderRequest request)
        {
            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            try
            {
                this.logger.LogInformation(
                    "Sending ACR SDK request to LWASv2: {PlatformName}, {BlobName}",
                    request.PlatformName,
                    request.BlobName);

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

                    this.logger.LogInformation(
                        "Received response from LWASv2: {Response}", responseString);

                    if (!string.IsNullOrEmpty(responseString) && responseString.EqualsIgnoreCase("Success$"))
                    {
                        return true;
                    }

                    this.logger.LogError(
                        "LWASv2 ACR SDK request unsuccessful. Response: {Response}",
                        responseString);
                }
            }
            catch (OperationCanceledException)
            {
                this.outputWriter.WriteLine("The LWASv2 ACR SDK request timed out.");
                this.logger.LogError("The LWASv2 ACR SDK request timed out.");
            }
            catch (Exception ex)
            {
                this.outputWriter.WriteLine(
                    $"Error communicating with LWASv2: {ex.Message}");
                this.logger.LogError(ex, "Error communicating with LWASv2.");
            }

            return false;
        }

        private class AcrSdkProviderRequest
        {
            public string PlatformName { get; set; }

            public string BlobName { get; set; }

            public IDictionary<string, string> UrlParameters { get; set; }
        }
    }
}
