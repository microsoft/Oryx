// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Base class for version providers that resolve the companion SDK version for a platform
    /// via a Unix socket to the external host.
    /// </summary>
    /// <remarks>
    /// Flow: Oryx → Unix socket → external host (LWASv2 OryxSdkImageProxy) → single SDK version response.
    /// Connects to the dedicated ACR SDK socket and sends <c>Action=get-version</c>.
    /// SDK pulling is handled separately by <see cref="ExternalAcrSdkProvider"/>.
    /// </remarks>
    public class ExternalAcrVersionProviderBase
    {
        private const string SocketPath = "/var/sdk-image-sockets/oryx-pull-sdk-image.socket";
        private const int MaxTimeoutForSocketOperationInSeconds = 120;

        private readonly ILogger logger;

        public ExternalAcrVersionProviderBase(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger(this.GetType());
        }

        /// <summary>
        /// Asks the external provider for the single SDK version to use for <paramref name="platformName"/>.
        /// </summary>
        /// <returns>The SDK version string, or <c>null</c> if the external provider could not resolve one.</returns>
        protected string GetCompanionSdkVersion(string platformName)

        {
            this.logger.LogInformation(
                "Requesting companion SDK version for {PlatformName} from external provider.",
                platformName);

            var version = this.SendRequestAsync(platformName).GetAwaiter().GetResult();

            if (!string.IsNullOrEmpty(version))
            {
                this.logger.LogInformation(
                    "External provider returned SDK version {Version} for {PlatformName}.",
                    version,
                    platformName);
            }
            else
            {
                this.logger.LogWarning(
                    "External provider returned no SDK version for {PlatformName}.",
                    platformName);
            }

            return version;
        }

        private async Task<string> SendRequestAsync(string platformName)
        {
            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            try
            {
                using (var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(MaxTimeoutForSocketOperationInSeconds)))
                {
                    await socket.ConnectAsync(new UnixDomainSocketEndPoint(SocketPath), cts.Token);
                    var requestJson = JsonSerializer.Serialize(
                        new { Action = "get-version", PlatformName = platformName }) + "$";
                    var requestBytes = Encoding.UTF8.GetBytes(requestJson);

                    await socket.SendAsync(new ArraySegment<byte>(requestBytes), SocketFlags.None, cts.Token);
                    var buffer = new byte[4096];
                    var received = await socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), SocketFlags.None, cts.Token);
                    var responseString = Encoding.UTF8.GetString(buffer, 0, received).TrimEnd('$');

                    if (!string.IsNullOrWhiteSpace(responseString))
                    {
                        return responseString.Trim();
                    }

                    this.logger.LogError("External provider returned an empty response.");
                }
            }
            catch (OperationCanceledException)
            {
                this.logger.LogError("Request to external provider timed out.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error communicating with external provider.");
            }

            return null;
        }
    }
}
