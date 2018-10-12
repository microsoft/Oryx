// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaultSourceRepoProvider : ISourceRepoProvider
    {
        private readonly ITempDirectoryProvider _tempDirectoryProvider;
        private readonly BuildScriptGeneratorOptions _options;
        private readonly ILogger<DefaultSourceRepoProvider> _logger;
        private bool _copiedToIntermediateDirectory = false;

        public DefaultSourceRepoProvider(
            ITempDirectoryProvider tempDirectoryProvider,
            IOptions<BuildScriptGeneratorOptions> options,
            ILogger<DefaultSourceRepoProvider> logger)
        {
            _tempDirectoryProvider = tempDirectoryProvider;
            _options = options.Value;
            _logger = logger;
        }

        public ISourceRepo GetSourceRepo()
        {
            if (_options.Inline)
            {
                return new LocalSourceRepo(_options.SourceDir);
            }

            var intermediateDir = EnsureIntermediateDirectory();

            if (!_copiedToIntermediateDirectory)
            {
                _logger.LogDebug(
                    $"Copying content from '{_options.SourceDir}' to '{intermediateDir.FullName}' ...");

                CopyDirectories(_options.SourceDir, intermediateDir.FullName, recursive: true);
                _copiedToIntermediateDirectory = true;
            }

            return new LocalSourceRepo(intermediateDir.FullName);
        }

        private DirectoryInfo EnsureIntermediateDirectory()
        {
            string intermediateDir;
            if (string.IsNullOrEmpty(_options.IntermediateDir))
            {
                intermediateDir = Path.Combine(_tempDirectoryProvider.GetTempDirectory(), "IntermediateDir");
            }
            else
            {
                intermediateDir = _options.IntermediateDir;
            }

            if (!Directory.Exists(intermediateDir))
            {
                _logger.LogDebug($"Creating intermediate directory at '{intermediateDir}' ...");
            }

            return Directory.CreateDirectory(intermediateDir);
        }

        private static void CopyDirectories(string sourceDirectory, string destinationDirectory, bool recursive)
        {
            // Get the subdirectories for the specified directory.
            var sourceDir = new DirectoryInfo(sourceDirectory);
            if (!sourceDir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirectory);
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            // Get the files in the source directory and copy them to the new location.
            var sourceDirFiles = sourceDir.GetFiles();
            foreach (var sourceDirFile in sourceDirFiles)
            {
                var destinationDirFile = Path.Combine(destinationDirectory, sourceDirFile.Name);
                sourceDirFile.CopyTo(destinationDirFile, overwrite: true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (recursive)
            {
                var sourceSubDirs = sourceDir.GetDirectories();
                foreach (var sourceSubDir in sourceSubDirs)
                {
                    var destinationSubDir = Path.Combine(destinationDirectory, sourceSubDir.Name);
                    CopyDirectories(sourceSubDir.FullName, destinationSubDir, recursive);
                }
            }
        }
    }
}