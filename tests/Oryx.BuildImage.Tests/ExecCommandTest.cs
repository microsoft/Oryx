// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Oryx.Tests.Common;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Xunit;

namespace Oryx.BuildImage.Tests
{
    public class ExecCommandTest
    {
        protected readonly DockerCli _dockerCli = new DockerCli();

        [Fact(Skip = "WIP")]
        public void Exec_Sanity()
        {
            // Arrange
            var appPath = "/tmp/app";
            var script = new ShellScriptBuilder()
                .AddCommand($"mkdir {appPath}")
                .CreateFile($"{appPath}/package.json", "{}")
                .AddCommand($"oryx exec {appPath} 'node --version' --debug")
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", "-c", script);

            // Assert
        }
    }
}
