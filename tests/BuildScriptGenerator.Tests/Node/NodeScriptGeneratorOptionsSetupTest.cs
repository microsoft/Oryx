// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Node;
using Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodeScriptGeneratorOptionsSetupTest
    {
        [Fact]
        public void Configure_SetsNodeVersion_ToLtsVersion_IfNoEnvironmentVariable_IsSet()
        {
            // Arrange
            var environment = new TestEnvironment();
            var optionsSetup = new NodeScriptGeneratorOptionsSetup(environment);
            var options = new NodeScriptGeneratorOptions();

            // Act
            optionsSetup.Configure(options);

            // Assert
            Assert.Equal("8.11.2", options.NodeJsDefaultVersion);
        }

        [Fact]
        public void Configure_SetsNodeVersion_ToEnvironmentVariableValue()
        {
            // Arrange
            var environment = new TestEnvironment();
            environment.Variables[NodeScriptGeneratorOptionsSetup.NodeJsDefaultVersion] = "10.10.10";
            var optionsSetup = new NodeScriptGeneratorOptionsSetup(environment);
            var options = new NodeScriptGeneratorOptions();

            // Act
            optionsSetup.Configure(options);

            // Assert
            Assert.Equal("10.10.10", options.NodeJsDefaultVersion);
        }
    }
}
