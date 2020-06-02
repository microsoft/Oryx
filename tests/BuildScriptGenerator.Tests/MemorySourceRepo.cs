// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    class MemorySourceRepo : ISourceRepo
    {
        private Dictionary<string, string> _pathsToFiles = new Dictionary<string, string>();

        public void AddFile(string content, params string[] paths)
        {
            var filePath = Path.Combine(paths);
            _pathsToFiles[filePath] = content;
        }

        public string RootPath => string.Empty;

        public bool FileExists(params string[] paths)
        {
            var path = Path.Combine(paths);
            return _pathsToFiles.ContainsKey(path);
        }

        public bool DirExists(params string[] paths)
        {
            var path = Path.Combine(paths);
            return _pathsToFiles.Keys.FirstOrDefault(x => x.StartsWith(path)) != null;
        }

        public string ReadFile(params string[] paths)
        {
            var path = Path.Combine(paths);
            try
            {
                return _pathsToFiles[path];
            }
            catch (KeyNotFoundException)
            {
                throw new FileNotFoundException("Path not found", path);
            }
        }

        public IEnumerable<string> EnumerateFiles(string searchPattern, bool searchSubDirectories)
        {
            return _pathsToFiles.Keys;
        }

        public string[] ReadAllLines(params string[] paths)
        {
            var content = ReadFile(paths);
            return content.Split(new[] { '\r', '\n' });
        }

        public string GetGitCommitId() => null;
    }
}