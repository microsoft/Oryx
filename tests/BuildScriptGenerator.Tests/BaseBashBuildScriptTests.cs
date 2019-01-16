// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class BaseBashBuildScriptTests
    {
        [Fact]
        public void ManyBuildScriptsShouldBeIncludedInOrder()
        {
            // Arrange
            const string script1 = "abcdefg";
            const string script2 = "123456";
            var scriptTemplate = new BaseBashBuildScript()
            {
                BuildScriptSnippets = new List<string>() { script1, script2 }
            };

            // Act
            var script = scriptTemplate.TransformText();

            // Assert
            script.Contains(script1 + Environment.NewLine + script2);
        }
    }
}