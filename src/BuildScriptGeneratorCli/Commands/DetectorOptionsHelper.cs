// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class DetectorOptionsHelper
    {
        public static void ConfigureDetectorOptions(
            DetectorOptions options,
            string sourceDir,
            string platform,
            string platformVersion,
            bool outputJson)

        {
            options.SourceDir = string.IsNullOrEmpty(sourceDir)
                ? Directory.GetCurrentDirectory() : Path.GetFullPath(sourceDir);
            options.PlatformName = platform;
            options.PlatformVersion = platformVersion;
            options.OutputJson = outputJson;
        }
    }
}