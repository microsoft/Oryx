// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.Detector.DotNetCore
{
    /// <summary>
    /// Gets the relative path to a project file, if it is present at the root of the given source directory.
    /// </summary>
    internal class RootDirectoryProjectFileProvider : IProjectFileProvider
    {
        private readonly ILogger<RootDirectoryProjectFileProvider> logger;

        public RootDirectoryProjectFileProvider(ILogger<RootDirectoryProjectFileProvider> logger)
        {
            this.logger = logger;
        }

        public string GetRelativePathToProjectFile(DetectorContext context)
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
