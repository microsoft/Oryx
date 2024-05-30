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
            return Environment.GetEnvironmentVariable("GIT_COMMIT");
        }
    }
}
