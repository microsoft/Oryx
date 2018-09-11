// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGenerator.SourceRepo
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Options;

    public class LocalSourceRepo : ISourceRepo
    {
        private readonly BuildScriptGeneratorOptions _options;

        public LocalSourceRepo(IOptions<BuildScriptGeneratorOptions> buildScriptGeneratorOptions)
        {
            _options = buildScriptGeneratorOptions.Value;

            if (string.IsNullOrEmpty(_options.SourcePath) || !Directory.Exists(_options.SourcePath))
            {
                throw new ArgumentException(
                    $"'{nameof(_options.SourcePath)}' must be a valid directory in the file system.");
            }
            RootPath = _options.SourcePath;
        }

        public string RootPath { get; }

        public bool FileExists(params string[] relativePathToFile)
        {
            var filePathInRepo = Path.Combine(relativePathToFile);
            var path = Path.Combine(RootPath, filePathInRepo);
            return File.Exists(path);
        }

        public string ReadFile(string relativePath)
        {
            var filePath = Path.Combine(RootPath, relativePath);
            return File.ReadAllText(filePath);
        }
    }
}