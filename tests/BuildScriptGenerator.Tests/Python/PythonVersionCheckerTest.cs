// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Python
{
    public class PythonVersionCheckerTest
    {
        [Fact]
        public void Checker_Warns_WhenOutdatedVersionUsed()
        {
            // Arrange
            var checker = new PythonVersionChecker(NullLogger<PythonVersionChecker>.Instance);

            // Act
            var messages = checker.CheckToolVersions(
                new Dictionary<string, string> { { PythonConstants.PythonName, PythonVersions.Python27Version } });

            // Assert
            Assert.Single(messages);
            Assert.Contains("outdated version of python was detected", messages.First().Content);
        }

        [Fact]
        public void Checker_DoesNotWarn_WhenLtsVersionUsed()
        {
            // Arrange
            var checker = new PythonVersionChecker(NullLogger<PythonVersionChecker>.Instance);

            // Act
            var ltsVer = PythonConstants.PythonLtsVersion;
            var messages = checker.CheckToolVersions(
                new Dictionary<string, string> { { PythonConstants.PythonName, ltsVer } });

            // Assert
            Assert.Empty(messages);
        }

        [Fact]
        public void Checker_DoesNotWarn_WhenCurrentVersionUsed()
        {
            // Arrange
            var checker = new PythonVersionChecker(NullLogger<PythonVersionChecker>.Instance);

            // Act
            var messages = checker.CheckToolVersions(
                new Dictionary<string, string> { { PythonConstants.PythonName, PythonVersions.Python37Version } });

            // Assert
            Assert.Empty(messages);
        }
    }
}
