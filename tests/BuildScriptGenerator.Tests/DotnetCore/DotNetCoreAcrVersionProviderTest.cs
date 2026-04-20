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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotnetCore
{
    public class DotNetCoreAcrVersionProviderTest
    {
        // --- Tag parsing ---

        [Fact]
        public void GetSupportedVersions_ParsesCompoundTags_IntoVersionMap()
        {
            // Arrange: tags in format {os}-{sdkVersion}_{runtimeVersion}
            var tags = new List<string>
            {
                "bookworm-8.0.403_8.0.18",
                "bookworm-9.0.100_9.0.0",
            };

            var provider = CreateProvider(tags, debianFlavor: "bookworm");

            // Act
            var versions = provider.GetSupportedVersions();

            // Assert
            Assert.NotNull(versions);
            Assert.Equal(2, versions.Count);
            Assert.Equal("8.0.403", versions["8.0.18"]);
            Assert.Equal("9.0.100", versions["9.0.0"]);
        }

        [Fact]
        public void GetSupportedVersions_SkipsTagsWithWrongFlavor()
        {
            var tags = new List<string>
            {
                "bookworm-8.0.403_8.0.18",
                "noble-9.0.100_9.0.0",
            };

            var provider = CreateProvider(tags, debianFlavor: "bookworm");
            var versions = provider.GetSupportedVersions();

            Assert.NotNull(versions);
            Assert.Single(versions);
            Assert.Equal("8.0.403", versions["8.0.18"]);
        }

        [Fact]
        public void GetSupportedVersions_SkipsMalformedTags_MissingUnderscore()
        {
            var tags = new List<string>
            {
                "bookworm-8.0.403_8.0.18",
                "bookworm-malformed-no-underscore",
            };

            var provider = CreateProvider(tags, debianFlavor: "bookworm");
            var versions = provider.GetSupportedVersions();

            Assert.NotNull(versions);
            Assert.Single(versions);
        }

        [Fact]
        public void GetSupportedVersions_ReturnsNull_WhenNoMatchingTags()
        {
            var tags = new List<string>
            {
                "noble-8.0.403_8.0.18",
            };

            var provider = CreateProvider(tags, debianFlavor: "bookworm");
            var versions = provider.GetSupportedVersions();

            // Returns null because the version map is empty (no tags match "bookworm" flavor)
            Assert.Null(versions);
        }

        [Fact]
        public void GetSupportedVersions_HandlesDuplicateRuntimeKeys()
        {
            // When multiple tags map to the same runtime, the last one wins (Dictionary behavior)
            var tags = new List<string>
            {
                "bookworm-8.0.301_8.0.18",
                "bookworm-8.0.403_8.0.18",
            };

            var provider = CreateProvider(tags, debianFlavor: "bookworm");
            var versions = provider.GetSupportedVersions();

            Assert.NotNull(versions);
            Assert.Single(versions);
            Assert.Equal("8.0.403", versions["8.0.18"]);
        }

        [Fact]
        public void GetDefaultRuntimeVersion_ReturnsNull_WhenNoVersionMap()
        {
            var provider = CreateProvider(new List<string>(), debianFlavor: "bookworm");
            var defaultVersion = provider.GetDefaultRuntimeVersion();
            Assert.Null(defaultVersion);
        }

        // --- Helpers ---

        private static TestDotNetCoreAcrVersionProvider CreateProvider(
            List<string> tags,
            string debianFlavor)
        {
            var options = Options.Create(new BuildScriptGeneratorOptions
            {
                DebianFlavor = debianFlavor,
                OryxAcrSdkRegistryUrl = "https://test.azurecr.io",
            });

            var handler = new StubHttpMessageHandler(tags);
            var httpClientFactory = new StubHttpClientFactory(handler);
            var ociClient = new OciRegistryClient("https://test.azurecr.io", httpClientFactory, NullLoggerFactory.Instance);

            return new TestDotNetCoreAcrVersionProvider(options, ociClient, NullLoggerFactory.Instance);
        }

        /// <summary>
        /// Test subclass that overrides HTTP behavior to return predefined tags.
        /// </summary>
        private class TestDotNetCoreAcrVersionProvider : DotNetCoreAcrVersionProvider
        {
            public TestDotNetCoreAcrVersionProvider(
                IOptions<BuildScriptGeneratorOptions> options,
                OciRegistryClient ociClient,
                ILoggerFactory loggerFactory)
                : base(options, ociClient, loggerFactory)
            {
            }
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
        /// Returns a tag list JSON response for any tags/list request.
        /// Returns 404 for token endpoints (simulating MCR-style public access).
        /// </summary>
        private class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly List<string> _tags;

            public StubHttpMessageHandler(List<string> tags)
            {
                _tags = tags;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var url = request.RequestUri.ToString();

                if (url.Contains("/tags/list"))
                {
                    var tagList = new OciTagList { Name = "test", Tags = _tags };
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(tagList)),
                    });
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
        }
    }
}
