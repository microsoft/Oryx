// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodeVersionCheckerTest
    {
        [Fact]
        public void Checker_WarnsWhenOutdatedVersionUsed()
        {
            // Arrange
            var checker = new NodeVersionChecker();

            // Act
            var messages = checker.CheckToolVersions(
                new Dictionary<string, string> { { NodeConstants.NodeJsName, "1.0.0" } });

            // Assert
            Assert.Single(messages);
            Assert.Contains("outdated version of Node.js was used", messages.First().Content);
        }

        [Fact]
        public void Checker_DoesNotWarnWhenLtsVersionUsed()
        {
            // Arrange
            var checker = new NodeVersionChecker();

            // Act
            var ltsVer = NodeScriptGeneratorOptionsSetup.NodeLtsVersion;
            var messages = checker.CheckToolVersions(
                new Dictionary<string, string> { { NodeConstants.NodeJsName, ltsVer } });

            // Assert
            Assert.Empty(messages);
        }

        [Fact]
        public void Checker_DoesNotWarnWhenCurrentVersionUsed()
        {
            // Arrange
            var checker = new NodeVersionChecker();

            // Act
            var messages = checker.CheckToolVersions(
                new Dictionary<string, string> { { NodeConstants.NodeJsName, "10.15.3" } });

            // Assert
            Assert.Empty(messages);
        }
    }
}
