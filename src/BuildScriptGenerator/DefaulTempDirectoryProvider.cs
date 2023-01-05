// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaulTempDirectoryProvider : ITempDirectoryProvider
    {
        private readonly string path;

        public DefaulTempDirectoryProvider()
        {
            // Create one unique subdirectory per session (or run of this tool)
            // Example structure:
            // /tmp/BuildScriptGenerator/guid1
            // /tmp/BuildScriptGenerator/guid2
            this.path = Path.Combine(
                Path.GetTempPath(),
                nameof(BuildScriptGenerator),
                Guid.NewGuid().ToString("N"));
        }

        public string GetTempDirectory()
        {
            // Ensure the temp directory is created
            Directory.CreateDirectory(this.path);
            return this.path;
        }
    }
}
