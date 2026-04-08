// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class SocketRequestHelperTest
    {
        [Fact]
        public async Task SendRequestAsync_ThrowsSocketException_WhenSocketDoesNotExist()
        {
            // Arrange — use a path that doesn't exist
            var nonExistentSocket = Path.Combine(Path.GetTempPath(), $"oryx-test-{Guid.NewGuid():N}.socket");

            // Act & Assert — should throw SocketException (no such file)
            await Assert.ThrowsAsync<SocketException>(
                () => SocketRequestHelper.SendRequestAsync(nonExistentSocket, new { Action = "test" }, timeoutSeconds: 5));
        }

        [Fact]
        public async Task SendRequestAsync_ReadsFullResponse_WithTerminator()
        {
            // This test verifies the '$' terminator loop works by using a real Unix socket pair.
            // Only runs on Unix-like systems where Unix sockets are available.
            if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            {
                return; // Skip on Windows — Unix domain sockets may not be available
            }

            var socketPath = Path.Combine(Path.GetTempPath(), $"oryx-test-{Guid.NewGuid():N}.socket");
            try
            {
                // Start a simple echo server that responds with "version-1.0$"
                using (var serverSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    serverSocket.Bind(new UnixDomainSocketEndPoint(socketPath));
                    serverSocket.Listen(1);

                    var serverTask = Task.Run(async () =>
                    {
                        using (var client = await serverSocket.AcceptAsync())
                        {
                            var buffer = new byte[4096];
                            var received = await client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                            // Read request and respond
                            var response = Encoding.UTF8.GetBytes("version-1.0$");
                            await client.SendAsync(new ArraySegment<byte>(response), SocketFlags.None);
                        }
                    });

                    // Act
                    var result = await SocketRequestHelper.SendRequestAsync(
                        socketPath, new { Action = "get-version" }, timeoutSeconds: 10);

                    // Assert — should include the terminator
                    Assert.Equal("version-1.0$", result);

                    await serverTask;
                }
            }
            finally
            {
                if (File.Exists(socketPath))
                {
                    File.Delete(socketPath);
                }
            }
        }

        [Fact]
        public async Task SendRequestAsync_ReturnsEmptyString_WhenServerClosesImmediately()
        {
            if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            {
                return;
            }

            var socketPath = Path.Combine(Path.GetTempPath(), $"oryx-test-{Guid.NewGuid():N}.socket");
            try
            {
                using (var serverSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    serverSocket.Bind(new UnixDomainSocketEndPoint(socketPath));
                    serverSocket.Listen(1);

                    var serverTask = Task.Run(async () =>
                    {
                        using (var client = await serverSocket.AcceptAsync())
                        {
                            // Read the request but close without responding
                            var buffer = new byte[4096];
                            await client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                            client.Shutdown(SocketShutdown.Both);
                        }
                    });

                    // Act
                    var result = await SocketRequestHelper.SendRequestAsync(
                        socketPath, new { Action = "test" }, timeoutSeconds: 10);

                    // Assert — EOF should return empty string
                    Assert.Equal(string.Empty, result);

                    await serverTask;
                }
            }
            finally
            {
                if (File.Exists(socketPath))
                {
                    File.Delete(socketPath);
                }
            }
        }

        [Fact]
        public async Task SendRequestAsync_ReadsFragmentedResponse()
        {
            if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            {
                return;
            }

            var socketPath = Path.Combine(Path.GetTempPath(), $"oryx-test-{Guid.NewGuid():N}.socket");
            try
            {
                using (var serverSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    serverSocket.Bind(new UnixDomainSocketEndPoint(socketPath));
                    serverSocket.Listen(1);

                    var serverTask = Task.Run(async () =>
                    {
                        using (var client = await serverSocket.AcceptAsync())
                        {
                            var buffer = new byte[4096];
                            await client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                            // Send response in fragments
                            await client.SendAsync(
                                new ArraySegment<byte>(Encoding.UTF8.GetBytes("ver")), SocketFlags.None);
                            await Task.Delay(50);
                            await client.SendAsync(
                                new ArraySegment<byte>(Encoding.UTF8.GetBytes("sion-2.0")), SocketFlags.None);
                            await Task.Delay(50);
                            await client.SendAsync(
                                new ArraySegment<byte>(Encoding.UTF8.GetBytes("$")), SocketFlags.None);
                        }
                    });

                    // Act
                    var result = await SocketRequestHelper.SendRequestAsync(
                        socketPath, new { Action = "test" }, timeoutSeconds: 10);

                    // Assert — fragmented response should be reassembled
                    Assert.Equal("version-2.0$", result);

                    await serverTask;
                }
            }
            finally
            {
                if (File.Exists(socketPath))
                {
                    File.Delete(socketPath);
                }
            }
        }
    }
}
