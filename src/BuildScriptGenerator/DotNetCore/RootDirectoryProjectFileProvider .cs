// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    class RootDirectoryProjectFileProvider : IProjectFileProvider
    {
        public string GetRelativePathToProjectFile(BuildScriptGeneratorContext context)
        {
            var sourceRepo = context.SourceRepo;

            // Check if root of the repo has a .csproj or a .fsproj file
            var projectFile = GetProjectFileAtRoot(sourceRepo, DotNetCoreConstants.CSharpProjectFileExtension) ??
                GetProjectFileAtRoot(sourceRepo, DotNetCoreConstants.FSharpProjectFileExtension);

            if (projectFile != null)
            {
                return new FileInfo(projectFile).Name;
            }

            return null;
        }

        private static string GetProjectFileAtRoot(ISourceRepo sourceRepo, string projectFileExtension)
        {
            return sourceRepo
                .EnumerateFiles($"*.{projectFileExtension}", searchSubDirectories: false)
                .FirstOrDefault();
        }
    }
}
