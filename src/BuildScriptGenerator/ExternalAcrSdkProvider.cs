// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
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
    /// External ACR-based SDK provider that communicates with LWASv2 over a Unix socket
    /// to pull SDK images from the WAWS Images ACR. This is the ACR equivalent of
    /// <see cref="ExternalSdkProvider"/> which uses blob storage.
    /// </summary>
    /// <remarks>
    /// Uses the same Unix socket path as <see cref="ExternalSdkProvider"/> but differentiates
    /// requests by setting <c>source=acr</c> in the UrlParameters. LWASv2's OryxProxy checks
    /// this parameter to route the request to ACR pull logic instead of blob download.
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

        public ExternalAcrSdkProvider(
            IStandardOutputWriter outputWriter,
            ILogger<ExternalAcrSdkProvider> logger,
            IOptions<BuildScriptGeneratorOptions> options)
        {
            this.logger = logger;
            this.outputWriter = outputWriter;
            this.options = options.Value;
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

            var repository = $"{SdkStorageConstants.AcrSdkRepositoryPrefix}/{platformName}";
            var tag = $"{debianFlavor}-{version}";

            // Construct a blob name for the output file path (same naming as blob-based downloads)
            var blobName = $"{platformName}-{debianFlavor}-{version}.tar.gz";

            this.logger.LogInformation(
                "Requesting SDK from ACR via LWASv2: platform={PlatformName}, version={Version}, " +
                "debianFlavor={DebianFlavor}, repository={Repository}, tag={Tag}",
                platformName, version, debianFlavor, repository, tag);
            this.outputWriter.WriteLine(
                $"Requesting SDK from ACR via external provider: {platformName} {version} ({debianFlavor})");

            // Check if the file is already cached locally
            var expectedFilePath = Path.Combine(ExternalSdksStorageDir, platformName, blobName);
            if (File.Exists(expectedFilePath))
            {
                this.logger.LogInformation(
                    "SDK already cached locally at {FilePath}, skipping ACR pull.",
                    expectedFilePath);
                this.outputWriter.WriteLine(
                    $"SDK already cached locally at {expectedFilePath}");
                return true;
            }

            try
            {
                var request = new AcrSdkProviderRequest
                {
                    PlatformName = platformName,
                    BlobName = blobName,
                    UrlParameters = new Dictionary<string, string>
                    {
                        { "source", "acr" },
                        { "repository", repository },
                        { "tag", tag },
                    },
                };

                var response = await this.SendRequestAsync(request);

                if (response && File.Exists(expectedFilePath))
                {
                    this.logger.LogInformation(
                        "Successfully pulled SDK from ACR via LWASv2: {PlatformName} {Version}, " +
                        "available at {FilePath}",
                        platformName, version, expectedFilePath);
                    this.outputWriter.WriteLine(
                        $"Successfully pulled SDK from ACR: {platformName} {version}");
                    return true;
                }
                else
                {
                    this.logger.LogWarning(
                        "ACR SDK pull via LWASv2 did not produce expected file: {PlatformName} {Version} " +
                        "at {FilePath}. Response: {Response}",
                        platformName, version, expectedFilePath, response);
                    this.outputWriter.WriteLine(
                        $"Failed to pull SDK from ACR via external provider: {platformName} {version}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex,
                    "Error requesting SDK from ACR via LWASv2: {PlatformName} {Version}",
                    platformName, version);
                this.outputWriter.WriteLine(
                    $"Error pulling SDK from ACR via external provider: {platformName} {version}: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SendRequestAsync(AcrSdkProviderRequest request)
        {
            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            try
            {
                this.logger.LogInformation(
                    "Sending ACR SDK request to external provider: {PlatformName}, {BlobName}, " +
                    "UrlParameters: {UrlParamsJson}",
                    request.PlatformName, request.BlobName,
                    JsonSerializer.Serialize(request.UrlParameters));

                using (var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(MaxTimeoutForSocketOperationInSeconds)))
                {
                    await socket.ConnectAsync(new UnixDomainSocketEndPoint(SocketPath), cts.Token);
                    var requestJson = JsonSerializer.Serialize(request);
                    this.logger.LogInformation(
                        "Connected to socket {SocketPath} and sending ACR request: {RequestJson}",
                        SocketPath, requestJson);

                    requestJson += "$";
                    var requestBytes = Encoding.UTF8.GetBytes(requestJson);

                    await socket.SendAsync(new ArraySegment<byte>(requestBytes), SocketFlags.None, cts.Token);
                    var buffer = new byte[4096];
                    var received = await socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), SocketFlags.None, cts.Token);
                    var responseString = Encoding.UTF8.GetString(buffer, 0, received);
                    this.logger.LogInformation(
                        "Received response from external ACR SDK provider: {Response}", responseString);

                    if (!string.IsNullOrEmpty(responseString) && responseString.EqualsIgnoreCase("Success$"))
                    {
                        return true;
                    }
                    else
                    {
                        this.logger.LogError(
                            "ACR SDK request to external provider was unsuccessful. Response: {Response}",
                            responseString);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                this.outputWriter.WriteLine("The external ACR SDK provider operation was canceled due to timeout.");
                this.logger.LogError("The external ACR SDK provider operation was canceled due to timeout.");
            }
            catch (Exception ex)
            {
                this.outputWriter.WriteLine(
                    $"Error communicating with external ACR SDK provider: {ex.Message}");
                this.logger.LogError(ex, "Error communicating with external ACR SDK provider.");
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
