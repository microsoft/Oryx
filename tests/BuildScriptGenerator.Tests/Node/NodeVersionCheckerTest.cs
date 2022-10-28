// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodeVersionCheckerTest
    {
        [Fact]
        public void Checker_Warns_WhenOutdatedVersionUsed()
        {
            // Arrange
            var commonOptions = Options.Create(new BuildScriptGeneratorOptions() { DebianFlavor = OsTypes.DebianBuster });
            var checker = new NodeVersionChecker(commonOptions, NullLogger<NodeVersionChecker>.Instance);

            // Act
            var messages = checker.CheckToolVersions(
                new Dictionary<string, string> { { NodeConstants.NodeToolName, "1.0.0" } });

            // Assert
            Assert.Single(messages);
            Assert.Contains("outdated version of node was detected", messages.First().Content);
        }

        [Fact]
        public void Checker_DoesNotWarn_WhenLtsVersionUsed()
        {
            // Arrange
            var commonOptions = Options.Create(new BuildScriptGeneratorOptions() { DebianFlavor = OsTypes.DebianBuster });
            var checker = new NodeVersionChecker(commonOptions, NullLogger<NodeVersionChecker>.Instance);

            // Act
            var ltsVer = NodeConstants.NodeLtsVersion;
            var messages = checker.CheckToolVersions(
                new Dictionary<string, string> { { NodeConstants.NodeToolName, ltsVer } });

            // Assert
            Assert.Empty(messages);
        }

        [Fact]
        public void Checker_DoesNotWarn_WhenCurrentVersionUsed()
        {
            // Arrange
            var commonOptions = Options.Create(new BuildScriptGeneratorOptions() { DebianFlavor = OsTypes.DebianBuster });
            var checker = new NodeVersionChecker(commonOptions, NullLogger<NodeVersionChecker>.Instance);

            // Act
            var messages = checker.CheckToolVersions(
                new Dictionary<string, string> { { NodeConstants.NodeToolName, NodeConstants.NodeLtsVersion } });

            // Assert
            Assert.Empty(messages);
        }
    }
}
