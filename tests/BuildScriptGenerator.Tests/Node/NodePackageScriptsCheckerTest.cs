// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodePackageScriptsCheckerTest
    {
        [Fact]
        public void Checker_DetectsGlobalNpmInstalls()
        {
            // Arrange
            var checker = new NodePackageScriptsChecker();
            var scripts = new Dictionary<string, string>
            {
                { "preinstall",     "npm i -g casperjs # checked and problematic" },
                { "install",        "echo bla bla      # checked and not problematic" },
                { "postshrinkwrap", "npm i -g casperjs # not checked" }
            };

            // Act & Assert
            Assert.Single(checker.CheckInstallScripts(scripts));
        }
    }
}
