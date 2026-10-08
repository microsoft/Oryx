// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class CapabilitiesCommandTest
    {
        [Fact]
        public void OnExecute_PrintsMachineReadableCapabilities()
        {
            var console = new TestConsole();

            var exitCode = CapabilitiesCommand.OnExecute(console, "json");

            Assert.Equal(ProcessConstants.ExitSuccess, exitCode);
            Assert.Equal(
                "{\"schemaVersion\":1,\"capabilities\":[\"deployment-progress/1\"]}",
                console.StdOutput.Trim());
            Assert.Empty(console.StdError);
        }

        [Fact]
        public void OnExecute_RejectsUnsupportedFormats()
        {
            var console = new TestConsole();

            var exitCode = CapabilitiesCommand.OnExecute(console, "text");

            Assert.Equal(ProcessConstants.ExitFailure, exitCode);
            Assert.Empty(console.StdOutput);
            Assert.Contains("--format json", console.StdError);
        }
    }
}
