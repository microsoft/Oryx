// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.Node;
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

        private class TestEnvironment : IEnvironment
        {
            public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

            public string GetEnvironmentVariable(string name)
            {
                if (Variables.TryGetValue(name, out var value))
                {
                    return value;
                }
                return null;
            }
        }
    }
}
