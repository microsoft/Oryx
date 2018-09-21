// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.IO;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class LocalSourceRepo : ISourceRepo
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

        public string ReadFile(params string[] paths)
        {
            var filePathInRepo = Path.Combine(paths);
            var path = Path.Combine(RootPath, filePathInRepo);
            return File.ReadAllText(path);
        }
    }
}