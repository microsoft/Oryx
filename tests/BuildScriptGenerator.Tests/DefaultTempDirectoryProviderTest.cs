// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DefaultTempDirectoryProviderTest
    {
        [Fact]
        public void GetTempDirectory_CreatesAndReturns_TemporaryDirectoryPath()
        {
            // Arrange
            var tempDirProvider = new DefaulTempDirectoryProvider();
            var expectedSubPath = Path.Combine(Path.GetTempPath(), nameof(BuildScriptGenerator));

            // Act
            var tempDir = tempDirProvider.GetTempDirectory();

            // Assert
            try
            {
                Assert.True(Directory.Exists(tempDir));
                Assert.StartsWith(expectedSubPath, tempDir);
            }
            finally
            {
                DeleteDirectory(tempDir);
            }
        }

        [Fact]
        public void Invoking_GetTempDirectory_MultipleTimes_ReturnsTheSameTempDirectory()
        {
            // Arrange
            var tempDirProvider = new DefaulTempDirectoryProvider();
            var expectedSubPath = Path.Combine(Path.GetTempPath(), nameof(BuildScriptGenerator));

            // Act
            var tempDir1 = tempDirProvider.GetTempDirectory();
            var tempDir2 = tempDirProvider.GetTempDirectory();

            // Assert
            try
            {
                Assert.Equal(tempDir1, tempDir2);
                Assert.StartsWith(expectedSubPath, tempDir1);
                Assert.True(Directory.Exists(tempDir1));
            }
            finally
            {
                DeleteDirectory(tempDir1);
                DeleteDirectory(tempDir2);
            }
        }

        private void DeleteDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, recursive: true);
            }
        }
    }
}
