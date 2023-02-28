// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Oryx.Automation.DotNet;
using Microsoft.Oryx.Automation.Services;
using Moq;
using Xunit;

namespace Microsoft.Oryx.Automation.Tests.DotNet
{
    public class DotNetAutomatorTests
    {
        private readonly Mock<IHttpClientFactory> httpClientFactoryMock;
        private readonly Mock<IVersionService> versionServiceMock;
        private readonly Mock<IFileService> fileReaderServiceMock;
        private readonly Mock<IYamlFileService> yamlFileReaderServiceMock;

        public DotNetAutomatorTests()
        {
            this.httpClientFactoryMock = new Mock<IHttpClientFactory>();
            this.versionServiceMock = new Mock<IVersionService>();
            this.yamlFileReaderServiceMock = new Mock<IYamlFileService>();
        }

        [Fact]
        public async Task RunAsync_Should_Call_HttpClient_GetOryxSdkVersionsAsync()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var expectedResponse = "Expected response string";
            httpClientMock.Setup(client => client.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedResponse)
                });

            this.httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(httpClientMock.Object);

            var dotNetAutomator = new DotNetAutomator(
                this.httpClientFactoryMock.Object,
                this.versionServiceMock.Object,
                this.fileReaderServiceMock.Object,
                this.yamlFileReaderServiceMock.Object);

            // Act
            await dotNetAutomator.RunAsync();

            // Assert
            httpClientMock.Verify(client => client.GetAsync($"{It.IsAny<string>()}/sdk-version"), expectedResponse);
        }
    }
}