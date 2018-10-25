// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;

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
            var filePathInRepo = Path.Combine(paths);
            var path = Path.Combine(RootPath, filePathInRepo);
            return File.Exists(path);
        }

        public IEnumerable<string> EnumerateFiles(string searchPattern, bool searchSubDirectories)
        {
            var searchOption = searchSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.EnumerateFiles(RootPath, searchPattern, searchOption);
        }

        public string ReadFile(params string[] paths)
        {
            var filePathInRepo = Path.Combine(paths);
            var path = Path.Combine(RootPath, filePathInRepo);
            return File.ReadAllText(path);
        }
    }
}