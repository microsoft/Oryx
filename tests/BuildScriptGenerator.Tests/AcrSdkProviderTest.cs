// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class AcrSdkProviderTest
    {
        // --- Constructor arg validation ---

        [Fact]
        public async Task RequestSdkFromAcrAsync_ThrowsForNullPlatformName()
        {
            var provider = CreateProvider();
            await Assert.ThrowsAsync<ArgumentException>(
                () => provider.RequestSdkFromAcrAsync(null, "1.0", "bookworm"));
        }

        [Fact]
        public async Task RequestSdkFromAcrAsync_ThrowsForEmptyPlatformName()
        {
            var provider = CreateProvider();
            await Assert.ThrowsAsync<ArgumentException>(
                () => provider.RequestSdkFromAcrAsync(string.Empty, "1.0", "bookworm"));
        }

        [Fact]
        public async Task RequestSdkFromAcrAsync_ThrowsForNullVersion()
        {
            var provider = CreateProvider();
            await Assert.ThrowsAsync<ArgumentException>(
                () => provider.RequestSdkFromAcrAsync("nodejs", null, "bookworm"));
        }

        [Fact]
        public async Task RequestSdkFromAcrAsync_ThrowsForEmptyVersion()
        {
            var provider = CreateProvider();
            await Assert.ThrowsAsync<ArgumentException>(
                () => provider.RequestSdkFromAcrAsync("nodejs", string.Empty, "bookworm"));
        }

        [Fact]
        public async Task RequestSdkFromAcrAsync_DefaultsDebianFlavor_WhenEmpty()
        {
            // Should NOT throw — debianFlavor defaults to "bookworm" when empty
            var handler = new FailingHandler();
            var provider = CreateProvider(handler);

            // Will fail at the HTTP level, but should not throw ArgumentException for empty debianFlavor
            var result = await provider.RequestSdkFromAcrAsync("nodejs", "20.0.0", string.Empty);
            Assert.False(result);
        }

        // --- Tag construction ---

        [Fact]
        public async Task RequestSdkFromAcrAsync_ConstructsSimpleTag_WhenNoRuntimeVersion()
        {
            // When runtimeVersion is null, tag should be "{flavor}-{version}"
            var handler = new TagCapturingHandler();
            var provider = CreateProvider(handler);

            await provider.RequestSdkFromAcrAsync("nodejs", "20.19.3", "bookworm");

            // The handler captures the manifest URL to verify tag construction
            Assert.Contains("bookworm-20.19.3", handler.LastManifestUrl);
            Assert.DoesNotContain("_", handler.LastManifestUrl);
        }

        [Fact]
        public async Task RequestSdkFromAcrAsync_ConstructsCompoundTag_WithRuntimeVersion()
        {
            // When runtimeVersion is provided, tag should be "{flavor}-{version}_{runtimeVersion}"
            var handler = new TagCapturingHandler();
            var provider = CreateProvider(handler);

            await provider.RequestSdkFromAcrAsync("dotnet", "8.0.403", "bookworm", "8.0.18");

            Assert.Contains("bookworm-8.0.403_8.0.18", handler.LastManifestUrl);
        }

        // --- Error handling ---

        [Fact]
        public async Task RequestSdkFromAcrAsync_ReturnsFalse_WhenManifestNotFound()
        {
            var handler = new FailingHandler();
            var provider = CreateProvider(handler);

            var result = await provider.RequestSdkFromAcrAsync("nodejs", "20.0.0", "bookworm");
            Assert.False(result);
        }

        [Fact]
        public async Task RequestSdkFromAcrAsync_ReturnsFalse_WhenManifestHasNoLayers()
        {
            // Arrange — HEAD returns digest OK, GET manifest returns manifest with no layers
            var handler = new ManifestHandler(layerDigest: null);
            var provider = CreateProvider(handler);

            // Act
            var result = await provider.RequestSdkFromAcrAsync("nodejs", "20.0.0", "bookworm");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RequestSdkFromAcrAsync_ReturnsFalse_WhenHttpThrows()
        {
            // Arrange — handler that throws on all requests
            var handler = new ThrowingHandler();
            var provider = CreateProvider(handler);

            // Act
            var result = await provider.RequestSdkFromAcrAsync("nodejs", "20.0.0", "bookworm");

            // Assert — exceptions are caught and return false
            Assert.False(result);
        }

        [Fact]
        public async Task RequestSdkFromAcrAsync_ReturnsFalse_WhenServerError()
        {
            // Arrange — 500 Internal Server Error
            var handler = new StatusCodeHandler(HttpStatusCode.InternalServerError);
            var provider = CreateProvider(handler);

            // Act
            var result = await provider.RequestSdkFromAcrAsync("nodejs", "20.0.0", "bookworm");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RequestSdkFromAcrAsync_UsesDefaultRepositoryPrefix()
        {
            // Arrange — verify the repository path includes the default "oryx" prefix
            var handler = new TagCapturingHandler();
            var provider = CreateProvider(handler);

            // Act
            await provider.RequestSdkFromAcrAsync("python", "3.11.0", "bookworm");

            // Assert — URL should contain "oryx/python-sdk"
            Assert.Contains("oryx/python-sdk", handler.LastManifestUrl);
        }

        // --- ExternalAcrSdkProvider arg validation ---

        [Fact]
        public async Task ExternalAcrSdkProvider_RequestSdkAsync_ThrowsForNullPlatformName()
        {
            var provider = new ExternalAcrSdkProvider(
                new DefaultStandardOutputWriter(),
                NullLogger<ExternalAcrSdkProvider>.Instance);

            await Assert.ThrowsAsync<ArgumentException>(
                () => provider.RequestSdkAsync(null, "1.0", "bookworm"));
        }

        [Fact]
        public async Task ExternalAcrSdkProvider_RequestSdkAsync_ThrowsForNullVersion()
        {
            var provider = new ExternalAcrSdkProvider(
                new DefaultStandardOutputWriter(),
                NullLogger<ExternalAcrSdkProvider>.Instance);

            await Assert.ThrowsAsync<ArgumentException>(
                () => provider.RequestSdkAsync("nodejs", null, "bookworm"));
        }

        [Fact]
        public async Task ExternalAcrSdkProvider_RequestSdkAsync_ThrowsForNullDebianFlavor()
        {
            var provider = new ExternalAcrSdkProvider(
                new DefaultStandardOutputWriter(),
                NullLogger<ExternalAcrSdkProvider>.Instance);

            await Assert.ThrowsAsync<ArgumentException>(
                () => provider.RequestSdkAsync("nodejs", "1.0", null));
        }

        // --- Helpers ---

        private static AcrSdkProvider CreateProvider(HttpMessageHandler handler = null)
        {
            var options = Options.Create(new BuildScriptGeneratorOptions
            {
                OryxAcrSdkRegistryUrl = "https://test.azurecr.io",
                DynamicInstallRootDir = "/tmp/oryx-test",
                DebianFlavor = "bookworm",
            });

            var httpHandler = handler ?? new FailingHandler();
            var factory = new StubHttpClientFactory(httpHandler);

            var ociClient = new OciRegistryClient("https://test.azurecr.io", factory, NullLoggerFactory.Instance);
            return new AcrSdkProvider(
                new DefaultStandardOutputWriter(),
                NullLogger<AcrSdkProvider>.Instance,
                options,
                ociClient);
        }

        private class StubHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpMessageHandler _handler;

            public StubHttpClientFactory(HttpMessageHandler handler)
            {
                _handler = handler;
            }

            public HttpClient CreateClient(string name)
            {
                return new HttpClient(_handler);
            }
        }

        /// <summary>
        /// Handler that returns 404 for all requests, simulating missing images.
        /// </summary>
        private class FailingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
        }

        /// <summary>
        /// Handler that captures the manifest URL and returns a 404 for the HEAD request.
        /// Used to verify tag construction without needing full OCI flow.
        /// </summary>
        private class TagCapturingHandler : HttpMessageHandler
        {
            public string LastManifestUrl { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var url = request.RequestUri.ToString();

                if (url.Contains("/manifests/"))
                {
                    LastManifestUrl = url;
                }

                // Return 404 to short-circuit — we only need to verify the URL was constructed correctly
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
        }

        /// <summary>
        /// Handler that throws on all requests, simulating network failures.
        /// </summary>
        private class ThrowingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new HttpRequestException("Simulated network failure");
            }
        }

        /// <summary>
        /// Handler that returns a specific status code for all requests.
        /// </summary>
        private class StatusCodeHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;

            public StatusCodeHandler(HttpStatusCode statusCode)
            {
                _statusCode = statusCode;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(_statusCode));
            }
        }

        /// <summary>
        /// Handler that simulates a manifest with optional layer digest.
        /// Returns digest on HEAD, manifest on GET, 404 on blob download.
        /// </summary>
        private class ManifestHandler : HttpMessageHandler
        {
            private readonly string _layerDigest;

            public ManifestHandler(string layerDigest)
            {
                _layerDigest = layerDigest;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var url = request.RequestUri.ToString();

                if (url.Contains("/manifests/") && request.Method == HttpMethod.Head)
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.OK);
                    resp.Headers.Add("Docker-Content-Digest", "sha256:testdigest");
                    return Task.FromResult(resp);
                }

                if (url.Contains("/manifests/") && request.Method == HttpMethod.Get)
                {
                    var manifest = _layerDigest != null
                        ? $"{{\"schemaVersion\":2,\"layers\":[{{\"digest\":\"{_layerDigest}\",\"size\":100}}]}}"
                        : "{\"schemaVersion\":2,\"layers\":[]}";
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(manifest),
                    });
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
        }
    }
}
