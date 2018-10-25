// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Python
{
    public class PythonScriptGeneratorOptionsSetupTest
    {
        [Fact]
        public void Configure_SetsPythonVersion_ToLatestVersion_IfNoEnvironmentVariable_IsSet()
        {
            // Arrange
            var environment = new TestEnvironment();
            var optionsSetup = new PythonScriptGeneratorOptionsSetup(environment);
            var options = new PythonScriptGeneratorOptions();

            // Act
            optionsSetup.Configure(options);

            // Assert
            Assert.Equal("3.7.0", options.PythonDefaultVersion);
        }

        [Fact]
        public void Configure_SetsPythonVersion_ToEnvironmentVariableValue()
        {
            // Arrange
            var environment = new TestEnvironment();
            environment.Variables[PythonScriptGeneratorOptionsSetup.PythonDefaultVersion] = "10.10.10";
            var optionsSetup = new PythonScriptGeneratorOptionsSetup(environment);
            var options = new PythonScriptGeneratorOptions();

            // Act
            optionsSetup.Configure(options);

            // Assert
            Assert.Equal("10.10.10", options.PythonDefaultVersion);
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
