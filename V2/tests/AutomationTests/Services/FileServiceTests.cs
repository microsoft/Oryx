// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using Microsoft.Oryx.Automation.Services;
using Moq;
using Xunit;

namespace Microsoft.Oryx.Automation.Tests.Services
{
    public class FileServiceTests
    {
        [Fact]
        public void UpdateVersionsToBuildTxt_ShouldUpdateFile()
        {
            // Arrange
            string platformName = "dotnet";
            string line = "1.0.0\n";
            var mockFileService = new Mock<IFileService>();
            mockFileService.Setup(x => x.UpdateVersionsToBuildTxt(platformName, line)).Verifiable();
            var fileService = mockFileService.Object;

            // Act
            fileService.UpdateVersionsToBuildTxt(platformName, line);

            // Assert
            mockFileService.VerifyAll();
        }
    }
}