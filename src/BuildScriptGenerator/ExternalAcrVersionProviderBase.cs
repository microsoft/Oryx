// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Base class for version providers that resolve the companion SDK version for a platform
    /// via a Unix socket to the external host.
    /// The external host dictates the SDK version to use for each platform.
    /// </summary>
    /// <remarks>
    /// Flow: Oryx → Unix socket → external host → single SDK version response.
    /// Connects to the dedicated ACR SDK socket and sends <c>Action=get-version</c>.
    /// SDK pulling is handled separately by <see cref="ExternalAcrSdkProvider"/>.
    /// </remarks>
    public class ExternalAcrVersionProviderBase
    {
        private const string SocketPath = "/var/sdk-image-sockets/oryx-pull-sdk-image.socket";
        private const int MaxTimeoutForSocketOperationInSeconds = 100;

        private readonly BuildScriptGeneratorOptions commonOptions;
        private readonly ILogger logger;
        private readonly IStandardOutputWriter outputWriter;

        public ExternalAcrVersionProviderBase(
            IOptions<BuildScriptGeneratorOptions> options,
            ILoggerFactory loggerFactory,
            IStandardOutputWriter outputWriter)
        {
            this.commonOptions = options.Value;
            this.logger = loggerFactory.CreateLogger(this.GetType());
            this.outputWriter = outputWriter;
        }

        /// <summary>
        /// Gets the Debian flavor from build options.
        /// </summary>
        protected string DebianFlavor => this.commonOptions.DebianFlavor;

        /// <summary>
        /// Asks the external provider for the single SDK version to use for <paramref name="platformName"/>.
        /// </summary>
        /// <returns>The SDK version string, or <c>null</c> if the external provider could not resolve one.</returns>
        protected string GetCompanionSdkVersion(string platformName, string debianFlavor)
        {
            this.logger.LogInformation(
                "Requesting companion SDK version for {PlatformName} and Debian flavor {DebianFlavor} from external ACR provider.",
                platformName,
                debianFlavor);

            string version;
            try
            {
                version = this.SendRequestAsync(platformName, debianFlavor).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Failed to get companion SDK version for {PlatformName} and Debian flavor {DebianFlavor} from external ACR provider.",
                    platformName,
                    debianFlavor);
                return null;
            }

            if (!string.IsNullOrEmpty(version))
            {
                this.logger.LogInformation(
                    "External ACR provider returned SDK version {Version} for {PlatformName} and Debian flavor {DebianFlavor}.",
                    version,
                    platformName,
                    debianFlavor);
                this.outputWriter.WriteLine(
                    $"External ACR provider resolved version '{version}' for {platformName}.");
            }
            else
            {
                this.logger.LogWarning(
                    "External ACR provider returned no SDK version for {PlatformName} and Debian flavor {DebianFlavor}.",
                    platformName,
                    debianFlavor);
                this.outputWriter.WriteLine(
                    $"External ACR version provider returned no version for {platformName}. Trying next provider.");
            }

            return version;
        }

        private async Task<string> SendRequestAsync(string platformName, string debianFlavor, string action = "get-version")
        {
            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            try
            {
                using (var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(MaxTimeoutForSocketOperationInSeconds)))
                {
                    await socket.ConnectAsync(new UnixDomainSocketEndPoint(SocketPath), cts.Token);
                    var requestJson = JsonSerializer.Serialize(
                        new { Action = action, PlatformName = platformName, DebianFlavor = debianFlavor }) + "$";
                    var requestBytes = Encoding.UTF8.GetBytes(requestJson);

                    await socket.SendAsync(new ArraySegment<byte>(requestBytes), SocketFlags.None, cts.Token);
                    var buffer = new byte[4096];
                    var received = await socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), SocketFlags.None, cts.Token);
                    var responseString = Encoding.UTF8.GetString(buffer, 0, received).TrimEnd('$');

                    if (!string.IsNullOrWhiteSpace(responseString) &&
                        !responseString.Equals("Error", StringComparison.OrdinalIgnoreCase))
                    {
                        return responseString.Trim();
                    }

                    this.logger.LogError(
                        "External provider returned an unsuccessful response: {Response}",
                        responseString);
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