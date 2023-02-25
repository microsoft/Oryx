// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using Microsoft.Oryx.Automation.Extensions;
using Moq;
using Moq.Protected;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Oryx.Automation.Tests.Extensions
{

    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task GetOryxSdkVersionsAsync_ReturnsExpectedVersions()
        {
            // Arrange
            string responseContent = @"<Versions><Version>1.0.0</Version><Version>2.0.0</Version></Versions>";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(responseContent) });
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            string url = "http://example.com/sdk-versions";

            // Act
            HashSet<string> versions = await httpClient.GetOryxSdkVersionsAsync(url);

            // Assert
            Assert.Equal(new HashSet<string> { "1.0.0", "2.0.0" }, versions);
        }

        [Fact]
        public async Task GetDataAsync_ReturnsResponseContent_WhenSuccessful()
        {
            // Arrange
            var httpClient = new HttpClient();
            var expectedContent = "Oryx!";
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expectedContent) };
            var handler = new TestHttpMessageHandler(response);
            var client = new HttpClient(handler);

            // Act
            var result = await client.GetDataAsync("https://example.com");

            // Assert
            Assert.Equal(expectedContent, result);
        }

        [Fact]
        public async Task GetDataAsync_ReturnsNull_WhenNotSuccessful()
        {
            // Arrange
            var httpClient = new HttpClient();
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var handler = new TestHttpMessageHandler(response);
            var client = new HttpClient(handler);

            // Act
            var result = await client.GetDataAsync("https://example.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOryxSdkVersionsAsync_ReturnsEmptyHashSet_WhenResponseIsNull()
        {
            // Arrange
            var response = @"<Versions></Versions>";
            var handler = new TestHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response) });
            var client = new HttpClient(handler);

            // Act
            var result = await client.GetOryxSdkVersionsAsync("https://examle.com");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOryxSdkVersionsAsync_ReturnsVersions_WhenResponseIsValidXml()
        {
            // Arrange
            var httpClient = new HttpClient();
            var response = @"<Versions>
                           <Version>1.0.0</Version>
                           <Version>2.0.0</Version>
                           <Version>3.0.0</Version>
                         </Versions>";
            var handler = new TestHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response) });
            var client = new HttpClient(handler);

            // Act
            var result = await client.GetOryxSdkVersionsAsync("https://example.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains("1.0.0", result);
            Assert.Contains("2.0.0", result);
            Assert.Contains("3.0.0", result);
        }

        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public TestHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                System.Threading.CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }
    }

}