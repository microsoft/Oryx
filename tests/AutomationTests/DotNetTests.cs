using Microsoft.Oryx.Automation.Client;
using Microsoft.Oryx.Automation.DotNet.Models;
using Microsoft.Oryx.Automation.Models;
using Microsoft.Oryx.Automation.Services;
using Microsoft.Oryx.Automation.Telemetry;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Oryx.Automation.DotNet.Tests
{
    public class DotNetTests
    {
        private DotNet dotNet;
        private Mock<IHttpClient> httpClientMock;
        private Mock<ILogger> loggerMock;
        private Mock<IVersionService> versionServiceMock;
        private Mock<IYamlFileReaderService> yamlFileReaderServiceMock;

        public DotNetTests()
        {
            this.httpClientMock = new Mock<IHttpClient>();
            this.loggerMock = new Mock<ILogger>();
            this.versionServiceMock = new Mock<IVersionService>();
            this.yamlFileReaderServiceMock = new Mock<IYamlFileReaderService>();

            this.dotNet = new DotNet(
                this.httpClientMock.Object,
                this.loggerMock.Object,
                this.versionServiceMock.Object,
                this.yamlFileReaderServiceMock.Object
            );
        }

        [Fact]
        public async Task GetOryxSdkVersionsAsync_ReturnsCorrectVersions()  
        {
            // Arrange
            string url = "https://someurl.com";
            string response = @"<Version>1.0.0</Version>";
            this.httpClientMock.Setup(h => h.GetDataAsync(url)).ReturnsAsync(response);

            // Act
            HashSet<string> result = await this.dotNet.GetOryxSdkVersionsAsync(url);

            // Assert
            Assert.Single(result);
            Assert.Contains("1.0.0", result);
        }
    }
}