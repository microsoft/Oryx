// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Microsoft.Oryx.Tests.Common;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodePackageScriptsCheckerTest
    {
        [Fact]
        public void Checker_DetectsGlobalNpmInstalls()
        {
            // Arrange
            var scripts = new Dictionary<string, string>
            {
                { "preinstall",     "npm i pkg -g # checked and problematic" },
                { "install",        "echo bla bla # checked and not problematic" },
                { "postshrinkwrap", "npm i -g pkg # not checked" }
            };

            // Act & Assert
            Assert.Single(NodePackageScriptsChecker.CheckScriptsForGlobalInstallationAttempts(scripts));
        }

        [Theory]
        [InlineData("npm install -g pkg",       true)]
        [InlineData("npm install pkg -g",       true)]
        [InlineData("npm install --global pkg", true)]
        [InlineData("npm install pkg --global", true)]
        [InlineData("npm i -g pkg",             true)]
        [InlineData("npm i pkg -g",             true)]
        [InlineData("npm i --global pkg",       true)]
        [InlineData("npm i pkg --global",       true)]
        [InlineData("cd abc && npm install -g", true)]
        [InlineData("npm install pkg",                false)]
        [InlineData("npm install pkg && grep -g bla", false)]
        [InlineData("npm install endswith-g",         false)]
        [InlineData("npm install endswith-global",    false)]
        [InlineData("npm install --globally bla",     false)]
        public void Checker_NpmGlobalPattern_MatchesCorrectly(string script, bool shouldMatch)
        {
            // Act & Assert
            Assert.Equal(shouldMatch, NodePackageScriptsChecker.NpmGlobalPattern.IsMatch(script));
        }
    }
}
