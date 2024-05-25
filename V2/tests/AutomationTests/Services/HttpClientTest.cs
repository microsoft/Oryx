// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using Microsoft.Oryx.Automation.Services;
using Moq.Protected;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using System.Text;

namespace Automation.Tests.Services
{
    public class HttpServiceTests
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
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            string url = "http://example.com/sdk-versions";

            // Create an instance of HttpService using the mocked HttpClientFactory
            HttpService httpService = new HttpService(httpClientFactoryMock.Object);

            // Act
            HashSet<string> versions = await httpService.GetOryxSdkVersionsAsync(url);

            // Assert
            Assert.Equal(new HashSet<string> { "1.0.0", "2.0.0" }, versions);
        }

        [Fact]
        public async Task GetDataAsync_ReturnsResponseContent_WhenSuccessful()
        {
            // Arrange
            var expectedContent = "Oryx!";
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expectedContent) };
            var handler = new TestHttpMessageHandler(response);
            var client = new HttpClient(handler);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);
            var service = new HttpService(httpClientFactoryMock.Object);

            // Act
            var result = await service.GetDataAsync("https://example.com");

            // Assert
            Assert.Equal(expectedContent, result);
        }

        [Fact]
        public async Task GetDataAsync_ReturnsNull_WhenNotSuccessful()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            var service = new HttpService(httpClientFactoryMock.Object);

            // Act
            var result = await service.GetDataAsync("https://example.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOryxSdkVersionsAsync_ReturnsEmptyHashSet_WhenResponseIsNull()
        {
            // Arrange
            var responseContent = @"<Versions></Versions>";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(responseContent, Encoding.UTF8, "application/xml") });
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            string url = "http://example.com/sdk-versions";

            var service = new HttpService(httpClientFactoryMock.Object);

            // Act
            HashSet<string> versions = await service.GetOryxSdkVersionsAsync(url);

            // Assert
            Assert.NotNull(versions);
            Assert.Empty(versions);
        }

        [Fact]
        public async Task GetOryxSdkVersionsAsync_ReturnsVersions_WhenResponseIsValidXml()
        {
            // Arrange
            string responseContent = @"<Versions>
                                        <Version>1.0.0</Version>
                                        <Version>2.0.0</Version>
                                        <Version>3.0.0</Version>
                                      </Versions>";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(responseContent) });
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            string url = "http://example.com/sdk-versions";

            var service = new HttpService(httpClientFactoryMock.Object);

            // Act
            HashSet<string> versions = await service.GetOryxSdkVersionsAsync(url);

            // Assert
            Assert.NotNull(versions);
            Assert.Equal(3, versions.Count);
            Assert.Contains("1.0.0", versions);
            Assert.Contains("2.0.0", versions);
            Assert.Contains("3.0.0", versions);
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
                CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }
    }
}