// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Php
{
    public class PhpScriptGeneratorOptionsSetupTest
    {
        [Fact]
        public void Configure_SetsPythonVersion_ToLatestVersion_WhenNoEnvironmentVariableSet()
        {
            // Arrange
            var environment = new TestEnvironment();
            var optionsSetup = new PhpScriptGeneratorOptionsSetup(environment);
            var options = new PhpScriptGeneratorOptions();

            // Act
            optionsSetup.Configure(options);

            // Assert
            Assert.Equal(PhpConstants.DefaultPhpRuntimeVersion, options.PhpDefaultVersion);
        }

        [Fact]
        public void Configure_SetsPythonVersion_ToEnvironmentVariableValue()
        {
            // Arrange
            var environment = new TestEnvironment();
            environment.Variables[PhpConstants.DefaultPhpRuntimeVersionEnvVarName] = "10.10.10";
            var optionsSetup = new PhpScriptGeneratorOptionsSetup(environment);
            var options = new PhpScriptGeneratorOptions();

            // Act
            optionsSetup.Configure(options);

            // Assert
            Assert.Equal("10.10.10", options.PhpDefaultVersion);
        }
    }
}
