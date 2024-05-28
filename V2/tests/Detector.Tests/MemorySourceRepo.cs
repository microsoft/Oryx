// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.Detector.Tests
{
    class MemorySourceRepo : ISourceRepo
    {
        private Dictionary<string, string> pathsToFiles = new Dictionary<string, string>();

        public MemorySourceRepo()
        {
        }
        public MemorySourceRepo(string sourceDirectory)
        {
            RootPath = sourceDirectory;
        }
        public string RootPath { get; }

        public void AddFile(string content, params string[] paths)
        {
            var filePath = Path.Combine(paths);
            pathsToFiles[filePath] = content;
        }

        public bool FileExists(params string[] paths)
        {
            var path = Path.Combine(paths);
            return pathsToFiles.ContainsKey(path);
        }

        public bool DirExists(params string[] paths)
        {
            var path = Path.Combine(paths);
            return pathsToFiles.Keys.FirstOrDefault(x => x.StartsWith(path)) != null;
        }

        public string ReadFile(params string[] paths)
        {
            var path = Path.Combine(paths);
            try
            {
                return pathsToFiles[path];
            }
            catch (KeyNotFoundException)
            {
                throw new FileNotFoundException("Path not found", path);
            }
        }

        public IEnumerable<string> EnumerateFiles(
            string searchPattern,
            bool searchSubDirectories,
            params string[] subDirectoryToSearchUnder)
        {
            throw new NotImplementedException();
        }

        public string[] ReadAllLines(params string[] paths)
        {
            var content = ReadFile(paths);
            return content.Split(new[] { '\r', '\n' });
        }

        public string GetGitCommitId() => null;
    }
}