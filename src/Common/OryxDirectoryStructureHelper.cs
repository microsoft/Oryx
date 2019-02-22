// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Oryx.Common
{
    public static class OryxDirectoryStructureHelper
    {
        private const int SourceRepoMaximumDepthToLog = 2;

        public static string GetDirectoryStructure(string sourcePath)
        {
            var log = string.Empty;
            if (!string.IsNullOrEmpty(sourcePath) && Directory.Exists(sourcePath))
            {
                var directoryStructureData = GetDirectoryStructureData(new DirectoryInfo(sourcePath), SourceRepoMaximumDepthToLog);
                log = directoryStructureData.ToString();
            }

            return log;
        }

        private static JToken GetDirectoryStructureData(DirectoryInfo sourceDirPath, int maxDepth)
        {
            return maxDepth <= 0
                ? string.Empty
                : JToken.FromObject(new
            {
                directory = sourceDirPath.EnumerateDirectories()
                    .ToDictionary(x => x.Name, x => GetDirectoryStructureData(x, maxDepth - 1)),
                file = sourceDirPath.EnumerateFiles().Select(x => x.Name).ToList()
            });
        }
    }
}
