// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Pulls SDK tarballs from ACR via an external host over a Unix socket.
    /// This is the ACR equivalent of <see cref="ExternalSdkProvider"/> (blob storage via socket).
    /// </summary>
    /// <remarks>
    /// Flow: Oryx → Unix socket → external host → ACR.
    /// Connects to a dedicated ACR SDK socket and sends <c>Action=pull-sdk</c> so the
    /// external host routes to the ACR image-pull logic.
    /// Version discovery is handled by <see cref="ExternalAcrVersionProviderBase"/>.
    /// </remarks>
    public class ExternalAcrSdkProvider : IExternalAcrSdkProvider
    {
        /// <summary>
        /// The directory where ACR-based SDKs are cached.
        /// Must match the mount path used by the external host.
        /// </summary>
        public const string ExternalAcrSdksStorageDir = "/var/OryxAcrSdks";

        private const string SocketPath = "/var/sdk-image-sockets/oryx-pull-sdk-image.socket";
        private const int MaxTimeoutForSocketOperationInSeconds = 100;

        private readonly ILogger<ExternalAcrSdkProvider> logger;
        private readonly IStandardOutputWriter outputWriter;

        public ExternalAcrSdkProvider(
            IStandardOutputWriter outputWriter,
            ILogger<ExternalAcrSdkProvider> logger)
        {
            this.logger = logger;
            this.outputWriter = outputWriter;
        }

        /// <inheritdoc/>
        /// Update this Method to pull SDK tarball from ACR via the external provider and place it in the local cache.
        public async Task<bool> RequestSdkAsync(string platformName, string version, string debianFlavor)
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
                throw new ArgumentException("Debian flavor cannot be null or empty.", nameof(debianFlavor));
            }

            // No local cache check here — external provider handles digest-based freshness internally.
            // On each call, the external provider does a lightweight HEAD request to compare the remote manifest for sdk image
            // digest against a local digest file, and only re-pulls the image if the digest changed.
            this.logger.LogInformation(
                "Requesting SDK from ACR via external provider: platform={PlatformName}, version={Version}, " +
                "debianFlavor={DebianFlavor}",
                platformName,
                version,
                debianFlavor);
            this.outputWriter.WriteLine(
                $"Requesting SDK from ACR via external provider: {platformName} {version} ({debianFlavor})");

            try
            {
                var request = new ExternalAcrSdkProviderRequest
                {
                    Action = "pull-sdk",
                    PlatformName = platformName,
                    Version = version,
                    DebianFlavor = debianFlavor,
                };

                var responseFilename = await this.SendRequestAsync(request);

                if (!string.IsNullOrEmpty(responseFilename))
                {
                    var filePath = Path.Combine(ExternalAcrSdksStorageDir, platformName, responseFilename);
                    this.logger.LogInformation(
                        "Successfully pulled SDK from ACR via external provider: {PlatformName} {Version}, " +
                        "available at {FilePath}",
                        platformName,
                        version,
                        filePath);
                    this.outputWriter.WriteLine(
                        $"Successfully pulled SDK from ACR via external provider: {platformName} {version}");
                    return true;
                }
                else
                {
                    this.logger.LogWarning(
                        "ACR SDK pull via external provider was unsuccessful: {PlatformName} {Version}",
                        platformName,
                        version);
                    this.outputWriter.WriteLine(
                        $"Failed to pull SDK from ACR via external provider: {platformName} {version}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Error requesting SDK from ACR via external provider: {PlatformName} {Version}",
                    platformName,
                    version);
                this.outputWriter.WriteLine(
                    $"Error pulling SDK from ACR via external provider: {platformName} {version}: {ex.Message}");
                return false;
            }
        }

        private async Task<string> SendRequestAsync(ExternalAcrSdkProviderRequest request)
        {
            try
            {
                this.logger.LogInformation(
                    "Sending ACR request via socket: Action={Action}, PlatformName={PlatformName}, Version={Version}, DebianFlavor={DebianFlavor}",
                    request.Action,
                    request.PlatformName,
                    request.Version,
                    request.DebianFlavor);

                var responseString = await SocketRequestHelper.SendRequestAsync(SocketPath, request, MaxTimeoutForSocketOperationInSeconds);
                responseString = responseString?.TrimEnd('$');

                this.logger.LogInformation(
                    "Received response from external ACR provider: {Response}", responseString);

                if (!string.IsNullOrEmpty(responseString) &&
                    !responseString.Equals("Error", StringComparison.OrdinalIgnoreCase))
                {
                    return responseString.Trim();
                }
                else
                {
                    this.logger.LogError(
                        "ACR request via socket was unsuccessful. Response: {Response}",
                        responseString);
                }
            }
            catch (OperationCanceledException)
            {
                this.outputWriter.WriteLine("The external ACR provider operation was canceled due to timeout.");
                this.logger.LogError("The external ACR provider operation was canceled due to timeout.");
            }
            catch (Exception ex)
            {
                this.outputWriter.WriteLine(
                    $"Error communicating with external ACR provider: {ex.Message}");
                this.logger.LogError(ex, "Error communicating with external ACR provider.");
            }

            return null;
        }

        private class ExternalAcrSdkProviderRequest
        {
            public string Action { get; set; }

            public string PlatformName { get; set; }

            public string Version { get; set; }

            public string DebianFlavor { get; set; }
        }
    }
}
