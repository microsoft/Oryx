// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    public static class OryxDirectoryStructureHelper
    {
        private const int SourceRepoMaximumDepthToLog = 2;
        private const int SourceRepoMaximumFileCountToLog = 1000;

        public static string GetDirectoryStructure(string sourcePath)
        {
            var log = string.Empty;
            if (!string.IsNullOrEmpty(sourcePath) && Directory.Exists(sourcePath))
            {
                var directoryStructureData = GetDirectoryStructureData(
                    new DirectoryInfo(sourcePath),
                    SourceRepoMaximumDepthToLog,
                    0);
                log = directoryStructureData.ToString();
            }

            return log;
        }

        private static JToken GetDirectoryStructureData(
            DirectoryInfo sourceDirPath,
            int maxDepth,
            int processedFiles = 0)
        {
            var currentDirfiles = sourceDirPath.EnumerateFiles().Count();

            if ((processedFiles + currentDirfiles) < SourceRepoMaximumFileCountToLog)
            {
                processedFiles = processedFiles + currentDirfiles;
            }
            else
            {
                currentDirfiles = SourceRepoMaximumFileCountToLog - processedFiles;
                processedFiles = SourceRepoMaximumFileCountToLog;
            }

            return maxDepth <= 0
                ? string.Empty
                : JToken.FromObject(new
                {
                    directory = sourceDirPath.EnumerateDirectories()
                    .ToDictionary(x => x.Name, x => GetDirectoryStructureData(x, maxDepth - 1, processedFiles)),
                    file = sourceDirPath.EnumerateFiles().Take(currentDirfiles).Select(x => x.Name).ToList(),
                });
        }
    }
}
