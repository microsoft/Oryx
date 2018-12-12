// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    class CachedSourceRepo : ISourceRepo
    {
        private Dictionary<string, string> pathToContent = new Dictionary<string, string>();

        public void AddFile(string content, params string[] paths)
        {
            var filePath = Path.Combine(paths);
            pathToContent[filePath] = content;
        }

        public string RootPath => string.Empty;

        public bool FileExists(params string[] paths)
        {
            var path = Path.Combine(paths);
            return pathToContent.ContainsKey(path);
        }

        public string ReadFile(params string[] paths)
        {
            var path = Path.Combine(paths);
            return pathToContent[path];
        }

        public IEnumerable<string> EnumerateFiles(string searchPattern, bool searchSubDirectories)
        {
            throw new System.NotImplementedException();
        }

        public string[] ReadAllLines(params string[] paths)
        {
            throw new System.NotImplementedException();
        }

        public string GetGitCommitId() => null;
    }
}