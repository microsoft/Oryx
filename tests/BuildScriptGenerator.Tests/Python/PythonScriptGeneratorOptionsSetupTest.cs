// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Python
{
    public class PythonScriptGeneratorOptionsSetupTest
    {
        [Fact]
        public void Configure_SetsPythonVersion_ToEnvironmentVariableValue()
        {
            // Arrange
            var environment = new TestEnvironment();
            environment.Variables[PythonConstants.PythonVersionEnvVarName] = "10.10.10";
            var optionsSetup = new PythonScriptGeneratorOptionsSetup(environment);
            var options = new PythonScriptGeneratorOptions();

            // Act
            optionsSetup.Configure(options);

            // Assert
            Assert.Equal("10.10.10", options.PythonVersion);
        }
    }
}
