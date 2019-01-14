// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DefaultEnvironmentTest
    {
        [Fact]
        public void EnvironmentVariableListShouldBeNullIfEmptyString()
        {
            // Arrange
            const string EnvVarName = "ORYX_TEST_VARIABLE";
            Environment.SetEnvironmentVariable(EnvVarName, "");
            var env = new DefaultEnvironment();

            // Act
            var valueList = env.GetEnvironmentVariableAsList(EnvVarName);

            //Assert
            Assert.Null(valueList);
        }

        [Fact]
        public void EnvironmentVariableListShouldTrimSpaces()
        {
            // Arrange
            const string EnvVarName = "ORYX_TEST_VARIABLE";
            Environment.SetEnvironmentVariable(EnvVarName, "1.3  ,   3.4");
            var env = new DefaultEnvironment();

            // Act
            var valueList = env.GetEnvironmentVariableAsList(EnvVarName);

            //Assert
            Assert.Equal(2, valueList.Count);
            Assert.True(valueList.Contains("1.3"));
            Assert.True(valueList.Contains("3.4"));
        }
    }
}