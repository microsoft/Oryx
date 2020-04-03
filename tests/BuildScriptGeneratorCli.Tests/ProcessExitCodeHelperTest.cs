// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class ProcessExitCodeHelperTest
    {
        [Fact]
        public void GetExitCodeForException_ReturnsExitCode_2_ForUnsupportedPlatformException()
        {
            // Arrange
            var expected = 2;

            // Act
            var actual = ProcessExitCodeHelper.GetExitCodeForException(new UnsupportedPlatformException("test"));

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetExitCodeForException_ReturnsExitCode_3_ForUnsupportedVersionException()
        {
            // Arrange
            var expected = 3;

            // Act
            var actual = ProcessExitCodeHelper.GetExitCodeForException(new UnsupportedVersionException("test"));

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetExitCodeForException_ReturnsExitCode_4_ForNoBuildStepException()
        {
            // Arrange
            var expected = 4;

            // Act
            var actual = ProcessExitCodeHelper.GetExitCodeForException(new NoBuildStepException("test"));

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
