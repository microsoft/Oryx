// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodeScriptGeneratorOptionsSetupTest
    {
        [Fact]
        public void Configure_SetsNodeVersion_ToEnvironmentVariableValue()
        {
            // Arrange
            var environment = new TestEnvironment();
            environment.Variables[NodeConstants.NodeVersion] = "10.10.10";
            var optionsSetup = new NodeScriptGeneratorOptionsSetup(environment);
            var options = new NodeScriptGeneratorOptions();

            // Act
            optionsSetup.Configure(options);

            // Assert
            Assert.Equal("10.10.10", options.NodeVersion);
        }
    }
}
