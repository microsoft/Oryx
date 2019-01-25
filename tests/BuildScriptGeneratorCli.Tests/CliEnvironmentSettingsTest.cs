// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Oryx.Tests.Common;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class CliEnvironmentSettingsTest
    {
        public static IEnumerable<object[]> DisableVariableNamesAndGetters
        {
            get
            {
                yield return new object[] { "DISABLE_DOTNETCORE_BUILD", (Func<CliEnvironmentSettings, bool>)(s => s.DisableDotNetCore) };
                yield return new object[] { "DISABLE_PYTHON_BUILD", (Func<CliEnvironmentSettings, bool>)(s => s.DisablePython) };
                yield return new object[] { "DISABLE_NODEJS_BUILD", (Func<CliEnvironmentSettings, bool>)(s => s.DisableNodeJs) };
                yield return new object[] { "DISABLE_MULTIPLATFORM_BUILD", (Func<CliEnvironmentSettings, bool>)(s => s.DisableMultiPlatformBuild) };
            }
        }

        [Theory]
        [MemberData(nameof(DisableVariableNamesAndGetters))]
        public void DisableFeature_DontDisable_IfSetToFalse(string envVariableName, Func<CliEnvironmentSettings, bool> valueGetter)
        {
            // Arrange
            var testEnvironment = new TestEnvironment();
            testEnvironment.Variables[envVariableName] = "false";
            var settingsProvider = new CliEnvironmentSettings(testEnvironment);

            // Act
            var value = valueGetter(settingsProvider);

            // Assert
            Assert.False(value);
        }

        [Theory]
        [MemberData(nameof(DisableVariableNamesAndGetters))]
        public void DisableFeature_DontDisable_IfSetToNonBoolean(string envVariableName, Func<CliEnvironmentSettings, bool> valueGetter)
        {
            // Arrange
            var testEnvironment = new TestEnvironment();
            testEnvironment.Variables[envVariableName] = "abc";
            var settingsProvider = new CliEnvironmentSettings(testEnvironment);

            // Act
            var value = valueGetter(settingsProvider);

            // Assert
            Assert.False(value);
        }

        [Theory]
        [MemberData(nameof(DisableVariableNamesAndGetters))]
        public void DisableFeature_DontDisable_IfNotSet(string envVariableName, Func<CliEnvironmentSettings, bool> valueGetter)
        {
            // Arrange
            var testEnvironment = new TestEnvironment();
            var settingsProvider = new CliEnvironmentSettings(testEnvironment);

            // Act
            var value = valueGetter(settingsProvider);

            // Assert
            Assert.False(value);
        }

        [Theory]
        [MemberData(nameof(DisableVariableNamesAndGetters))]
        public void DisableFeature_Disable_IfSetToTrue(string envVariableName, Func<CliEnvironmentSettings, bool> valueGetter)
        {
            // Arrange
            var testEnvironment = new TestEnvironment();
            testEnvironment.Variables[envVariableName] = "true";
            var settingsProvider = new CliEnvironmentSettings(testEnvironment);

            // Act
            var value = valueGetter(settingsProvider);

            // Assert
            Assert.True(value);
        }
    }
}