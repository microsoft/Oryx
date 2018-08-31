// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.SourceRepo
{
    using System;
    using System.IO;

    public class LocalSourceRepo : ISourceRepo
    {
        public string RootPath { get; private set; }

        public LocalSourceRepo(string _sourcePath)
        {
            if (string.IsNullOrEmpty(_sourcePath) || !Directory.Exists(_sourcePath))
            {
                throw new ArgumentException($"{nameof(_sourcePath)} must be a valid directory in the file system.");
            }
            RootPath = _sourcePath;
        }

        public bool FileExists(params string[] pathToFile)
        {
            var filePathInRepo = Path.Combine(pathToFile);
            var path = Path.Combine(RootPath, filePathInRepo);
            return File.Exists(path);
        }

        public string ReadFile(string path)
        {
            var filePath = Path.Combine(RootPath, path);
            return File.ReadAllText(filePath);
        }
    }
}