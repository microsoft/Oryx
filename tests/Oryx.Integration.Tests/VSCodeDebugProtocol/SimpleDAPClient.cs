// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Oryx.Integration.Tests.VSCodeDebugProtocol
{
    /// <summary>
    /// A very naive implementation of the Debug Adapter Protocol.
    /// Supports the Initialize request only.
    /// Full protocol specifications:
    /// https://microsoft.github.io/debug-adapter-protocol/specification
    /// </summary>
    public class SimpleDAPClient : IDisposable
    {
        private const string TwoCRLF = "\r\n\r\n";
        private const string CLHeader = "Content-Length";
        private readonly Encoding StreamEncoding = Encoding.UTF8;
        private readonly JsonSerializerSettings IgnoreNullsSerializerSettings =
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        private readonly string _name;
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _tcpStream;
        private uint _sequence = 1;

        public SimpleDAPClient(string hostname, int port, string name)
        {
            _name = name;
            _tcpClient = new TcpClient(hostname, port);
            _tcpStream = _tcpClient.GetStream();
        }

        public async Task<dynamic> Initialize()
        {
            var reqArgs = new Messages.InitializeRequestArguments { ClientName = _name };
            var req = new Messages.InitializeRequest { SequenceNumber = _sequence++, Args = reqArgs };

            // Serialize the request to a buffer
            var reqBody = JsonConvert.SerializeObject(req, Formatting.None, IgnoreNullsSerializerSettings);
            var reqBodyLen = StreamEncoding.GetByteCount(reqBody);
            byte[] reqData = StreamEncoding.GetBytes($"{CLHeader}: {reqBodyLen}\r\n\r\n{reqBody}");

            // Write out the request
            await _tcpStream.WriteAsync(reqData, 0, reqData.Length);

            // Read the anticipated response
            var rawMessages = await RecvChunks();
            try
            {
                return JsonConvert.DeserializeObject(GetMessageBody(rawMessages.First()));
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        private static string GetMessageBody(string rawMessage)
        {
            var headerIndex = rawMessage.IndexOf(CLHeader);
            if (headerIndex != -1)
            {
                rawMessage = rawMessage.Substring(0, headerIndex);
            }
            return rawMessage;
        }

        private static bool DoesNotStartWithCLHeader(string chunk)
        {
            return !chunk.StartsWith(CLHeader, StringComparison.Ordinal);
        }

        /// <summary>
        /// An overly simplistic implementation of the protocol.
        /// For example - ignores the Content-Length headers completely.
        /// </summary>
        /// <returns>Array of raw messages received.</returns>
        private async Task<IEnumerable<string>> RecvChunks()
        {
            var rawResData = new byte[256];
            int bytesRecvd = await _tcpStream.ReadAsync(rawResData, 0, rawResData.Length);

            string resData = StreamEncoding.GetString(rawResData, 0, bytesRecvd);
            string[] resChunks = resData.Split(TwoCRLF);

            return resChunks.Where(DoesNotStartWithCLHeader);
        }

        public void Dispose()
        {
            _tcpStream.Close();
            _tcpClient.Close();
        }
    }
}