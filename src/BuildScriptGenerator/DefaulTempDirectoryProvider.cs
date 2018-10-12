// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;
using System.IO;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class DefaulTempDirectoryProvider : ITempDirectoryProvider
    {
        private readonly string _path;

        public DefaulTempDirectoryProvider()
        {
            // Create one unique subdirectory per session (or run of this tool)
            // Example structure:
            // /tmp/BuildScriptGenerator/guid1
            // /tmp/BuildScriptGenerator/guid2
            _path = Path.Combine(
                Path.GetTempPath(),
                nameof(BuildScriptGenerator),
                Guid.NewGuid().ToString("N"));
        }

        public string GetTempDirectory()
        {
            // Ensure the temp directory is created
            Directory.CreateDirectory(_path);
            return _path;
        }
    }
}