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

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Handles Unix domain socket communication used by the external SDK providers.
    /// Encapsulates the connect → send → receive loop with '$' terminator protocol.
    /// </summary>
    public static class SocketRequestHelper
    {
        private const int DefaultTimeoutSeconds = 100;
        private const int BufferSize = 4096;

        /// <summary>
        /// Sends a JSON-serialized request to a Unix domain socket and returns the raw response.
        /// The protocol appends '$' as a message terminator for both request and response.
        /// </summary>
        /// <param name="socketPath">Path to the Unix domain socket.</param>
        /// <param name="request">Object to JSON-serialize and send.</param>
        /// <param name="timeoutSeconds">Timeout for the entire operation.</param>
        /// <returns>The raw response string (including '$' terminator if present), or empty on EOF.</returns>
        public static async Task<string> SendRequestAsync(
            string socketPath,
            object request,
            int timeoutSeconds = DefaultTimeoutSeconds)
        {
            using (var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds)))
            {
                await socket.ConnectAsync(new UnixDomainSocketEndPoint(socketPath), cts.Token);

                var requestJson = JsonSerializer.Serialize(request) + "$";
                var requestBytes = Encoding.UTF8.GetBytes(requestJson);

                await socket.SendAsync(new ArraySegment<byte>(requestBytes), SocketFlags.None, cts.Token);

                // Read until '$' terminator — TCP may fragment the response across multiple reads.
                var responseBuilder = new StringBuilder();
                var buffer = new byte[BufferSize];
                while (true)
                {
                    var received = await socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), SocketFlags.None, cts.Token);
                    if (received == 0)
                    {
                        break;
                    }

                    responseBuilder.Append(Encoding.UTF8.GetString(buffer, 0, received));
                    if (responseBuilder.Length > 0 && responseBuilder[responseBuilder.Length - 1] == '$')
                    {
                        break;
                    }
                }

                return responseBuilder.ToString();
            }
        }
    }
}
