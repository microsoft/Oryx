// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.Common.Utilities;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class LocalSourceRepo : ISourceRepo
    {
        public LocalSourceRepo(string sourceDirectory)
        {
            RootPath = sourceDirectory;
        }

        public string RootPath { get; }

        public bool FileExists(params string[] paths)
        {
            var path = ResolvePath(paths);
            return File.Exists(path);
        }

        public IEnumerable<string> EnumerateFiles(string searchPattern, bool searchSubDirectories)
        {
            var searchOption = searchSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.EnumerateFiles(RootPath, searchPattern, searchOption);
        }

        public string ReadFile(params string[] paths)
        {
            var path = ResolvePath(paths);
            return File.ReadAllText(path);
        }

        public string[] ReadAllLines(params string[] paths)
        {
            var path = ResolvePath(paths);
            return File.ReadAllLines(path);
        }

        public string GetGitCommitId()
        {
            (int exitCode, string output, string error) = ProcessHelper.RunProcess("git", new string[] { "rev-parse", "HEAD" }, this.RootPath, 2);

            if (exitCode != 0)
            {
                return null;
            }

            return output?.Trim();
        }

        private string ResolvePath(params string[] paths)
        {
            var filePathInRepo = Path.Combine(paths);
            return Path.Combine(RootPath, filePathInRepo);
        }
    }
}