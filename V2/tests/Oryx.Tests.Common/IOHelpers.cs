// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;

namespace Microsoft.Oryx.Tests.Common
{
    public static class IOHelpers
    {
        public static string CreateTempDir(string parentPath)
        {
            return Directory.CreateDirectory(Path.Combine(parentPath, Guid.NewGuid().ToString("N"))).FullName;
        }

        public static void CreateFile(string dirPath, string fileContent, params string[] filePathParts)
        {
            File.WriteAllText(Path.Combine(dirPath, Path.Combine(filePathParts)), fileContent);
        }
    }
}
