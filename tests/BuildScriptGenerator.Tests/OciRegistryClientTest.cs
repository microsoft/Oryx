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
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class OciRegistryClientTest
    {
        // --- Constructor validation ---

        [Fact]
        public void Constructor_ThrowsForEmptyRegistryUrl()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new OciRegistryClient(string.Empty, new MockHttpClientFactory(), NullLoggerFactory.Instance));
            Assert.Contains("Registry URL", ex.Message);
        }

        [Fact]
        public void Constructor_ThrowsForNonHttpsUrl()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new OciRegistryClient("http://insecure.io", new MockHttpClientFactory(), NullLoggerFactory.Instance));
            Assert.Contains("HTTPS", ex.Message);
        }

        [Fact]
        public void Constructor_AcceptsValidHttpsUrl()
        {
            var client = new OciRegistryClient(
                "https://myregistry.azurecr.io",
                new MockHttpClientFactory(),
                NullLoggerFactory.Instance);
            Assert.NotNull(client);
        }

        // --- GetFirstLayerDigest ---

        [Fact]
        public void GetFirstLayerDigest_ReturnsNull_WhenManifestIsNull()
        {
            Assert.Null(OciRegistryClient.GetFirstLayerDigest(null));
        }

        [Fact]
        public void GetFirstLayerDigest_ReturnsNull_WhenLayersAreEmpty()
        {
            var manifest = new OciManifest { Layers = new List<OciDescriptor>() };
            Assert.Null(OciRegistryClient.GetFirstLayerDigest(manifest));
        }

        [Fact]
        public void GetFirstLayerDigest_ReturnsNull_WhenLayersAreNull()
        {
            var manifest = new OciManifest { Layers = null };
            Assert.Null(OciRegistryClient.GetFirstLayerDigest(manifest));
        }

        [Fact]
        public void GetFirstLayerDigest_ReturnsFirstDigest()
        {
            var manifest = new OciManifest
            {
                Layers = new List<OciDescriptor>
                {
                    new OciDescriptor { Digest = "sha256:abc123" },
                    new OciDescriptor { Digest = "sha256:def456" },
                },
            };
            Assert.Equal("sha256:abc123", OciRegistryClient.GetFirstLayerDigest(manifest));
        }

        // --- GetAllTagsAsync ---

        [Fact]
        public async Task GetAllTagsAsync_ReturnsTags_FromSinglePage()
        {
            var tagList = new OciTagList { Name = "test/repo", Tags = new List<string> { "v1", "v2" } };
            var handler = new MockHttpMessageHandler();
            handler.AddResponse(
                "https://myregistry.azurecr.io/v2/test/repo/tags/list",
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(tagList)),
                });

            var client = CreateClient(handler);
            var tags = await client.GetAllTagsAsync("test/repo");
            Assert.Equal(new List<string> { "v1", "v2" }, tags);
        }

        [Fact]
        public async Task GetAllTagsAsync_HandlesPagination()
        {
            var page1 = new OciTagList { Name = "test/repo", Tags = new List<string> { "v1" } };
            var page2 = new OciTagList { Name = "test/repo", Tags = new List<string> { "v2" } };

            var handler = new MockHttpMessageHandler();
            var resp1 = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(page1)),
            };
            resp1.Headers.Add("Link", "</v2/test/repo/tags/list?last=v1>; rel=\"next\"");
            handler.AddResponse("https://myregistry.azurecr.io/v2/test/repo/tags/list", resp1);
            handler.AddResponse(
                "https://myregistry.azurecr.io/v2/test/repo/tags/list?last=v1",
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(page2)),
                });

            var client = CreateClient(handler);
            var tags = await client.GetAllTagsAsync("test/repo");
            Assert.Equal(new List<string> { "v1", "v2" }, tags);
        }

        [Fact]
        public async Task GetAllTagsAsync_ThrowsOnFailure()
        {
            var handler = new MockHttpMessageHandler();
            handler.AddResponse(
                "https://myregistry.azurecr.io/v2/test/repo/tags/list",
                new HttpResponseMessage(HttpStatusCode.NotFound));

            var client = CreateClient(handler);
            await Assert.ThrowsAsync<HttpRequestException>(() => client.GetAllTagsAsync("test/repo"));
        }

        // --- GetManifestAsync ---

        [Fact]
        public async Task GetManifestAsync_ParsesManifest()
        {
            var manifest = new OciManifest
            {
                SchemaVersion = 2,
                Layers = new List<OciDescriptor>
                {
                    new OciDescriptor { Digest = "sha256:layer1", Size = 1234 },
                },
            };

            var handler = new MockHttpMessageHandler();
            handler.AddResponse(
                "https://myregistry.azurecr.io/v2/repo/manifests/latest",
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(manifest)),
                });

            var client = CreateClient(handler);
            var result = await client.GetManifestAsync("repo", "latest");
            Assert.NotNull(result);
            Assert.Equal(2, result.SchemaVersion);
            Assert.Single(result.Layers);
            Assert.Equal("sha256:layer1", result.Layers[0].Digest);
        }

        // --- Token TTL ---

        [Fact]
        public async Task GetAllTagsAsync_UsesTokenFromCache_WhenNotExpired()
        {
            var tagList = new OciTagList { Tags = new List<string> { "v1" } };
            var registryHost = "custom.azurecr.io";
            var baseUrl = $"https://{registryHost}";

            var handler = new MockHttpMessageHandler();
            // Token endpoint returns token with 600s lifetime
            handler.AddResponse(
                $"{baseUrl}/oauth2/token",
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"access_token\":\"tok1\",\"expires_in\":600}"),
                });
            // First tags request
            handler.AddResponse(
                $"{baseUrl}/v2/repo/tags/list",
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(tagList)),
                });

            var client = CreateClient(handler, registryHost: registryHost);

            // First call — acquires token
            await client.GetAllTagsAsync("repo");
            int tokenCallsAfterFirst = handler.GetCallCount($"{baseUrl}/oauth2/token");

            // Second tags response
            handler.AddResponse(
                $"{baseUrl}/v2/repo/tags/list",
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(tagList)),
                });

            // Second call — should use cached token
            await client.GetAllTagsAsync("repo");
            int tokenCallsAfterSecond = handler.GetCallCount($"{baseUrl}/oauth2/token");

            // Token endpoint should only have been called once
            Assert.Equal(tokenCallsAfterFirst, tokenCallsAfterSecond);
        }

        // --- GetManifestDigestAsync ---

        [Fact]
        public async Task GetManifestDigestAsync_ReturnsDigest()
        {
            var handler = new MockHttpMessageHandler();
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Headers.Add("Docker-Content-Digest", "sha256:abc123");
            handler.AddResponse("https://myregistry.azurecr.io/v2/repo/manifests/v1", resp);

            var client = CreateClient(handler);
            var digest = await client.GetManifestDigestAsync("repo", "v1");
            Assert.Equal("sha256:abc123", digest);
        }

        [Fact]
        public async Task GetManifestDigestAsync_ReturnsNull_OnFailure()
        {
            var handler = new MockHttpMessageHandler();
            handler.AddResponse(
                "https://myregistry.azurecr.io/v2/repo/manifests/v1",
                new HttpResponseMessage(HttpStatusCode.NotFound));

            var client = CreateClient(handler);
            var digest = await client.GetManifestDigestAsync("repo", "v1");
            Assert.Null(digest);
        }

        // --- Helpers ---

        private static OciRegistryClient CreateClient(
            MockHttpMessageHandler handler,
            string registryHost = "myregistry.azurecr.io")
        {
            var url = $"https://{registryHost}";
            var factory = new MockHttpClientFactory(handler);
            return new OciRegistryClient(url, factory, NullLoggerFactory.Instance);
        }

        private class MockHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpMessageHandler _handler;

            public MockHttpClientFactory()
            {
                _handler = new MockHttpMessageHandler();
            }

            public MockHttpClientFactory(HttpMessageHandler handler)
            {
                _handler = handler;
            }

            public HttpClient CreateClient(string name)
            {
                return new HttpClient(_handler);
            }
        }

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly Dictionary<string, Queue<HttpResponseMessage>> _responses
                = new Dictionary<string, Queue<HttpResponseMessage>>(StringComparer.OrdinalIgnoreCase);

            private readonly Dictionary<string, int> _callCounts
                = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            public void AddResponse(string urlPrefix, HttpResponseMessage response)
            {
                if (!_responses.ContainsKey(urlPrefix))
                {
                    _responses[urlPrefix] = new Queue<HttpResponseMessage>();
                }

                _responses[urlPrefix].Enqueue(response);
            }

            public int GetCallCount(string urlPrefix)
            {
                foreach (var kvp in _callCounts)
                {
                    if (kvp.Key.StartsWith(urlPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return kvp.Value;
                    }
                }

                return 0;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var url = request.RequestUri.ToString();

                // Track call counts
                if (!_callCounts.ContainsKey(url))
                {
                    _callCounts[url] = 0;
                }

                _callCounts[url]++;

                // Find matching response by URL prefix
                foreach (var kvp in _responses)
                {
                    if (url.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase) && kvp.Value.Count > 0)
                    {
                        return Task.FromResult(kvp.Value.Dequeue());
                    }
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
        }
    }
}
