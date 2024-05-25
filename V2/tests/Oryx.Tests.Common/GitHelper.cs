// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Xunit;

namespace Microsoft.Oryx.Tests.Common
{
    public static class GitHelper
    {
        public static string GetCommitID()
        {
            var workingDirectory = (new FileInfo(Assembly.GetExecutingAssembly().Location)).Directory.FullName;

            (int exitCode, string output, string error) = ProcessHelper.RunProcess(
                "git",
                new[] { "rev-parse", "HEAD" },
                workingDirectory,
                waitTimeForExit: TimeSpan.FromSeconds(10));

            Assert.Equal(0, exitCode);

            var gitCommitID = output.Trim().ReplaceNewLine();
            return gitCommitID;
        }
    }
}
