// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal class DefaultSourceRepoProvider : ISourceRepoProvider
    {
        private readonly ITempDirectoryProvider _tempDirectoryProvider;
        private readonly string _sourceDirectory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<DefaultSourceRepoProvider> _logger;
        private LocalSourceRepo _sourceRepo;

        public DefaultSourceRepoProvider(
            ITempDirectoryProvider tempDirectoryProvider,
            IOptions<BuildScriptGeneratorOptions> options,
            ILoggerFactory loggerFactory)
        {
            _tempDirectoryProvider = tempDirectoryProvider;
            var genOptions = options.Value;
            _sourceDirectory = genOptions.SourceDir;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<DefaultSourceRepoProvider>();
        }

        public ISourceRepo GetSourceRepo()
        {
            if (_sourceRepo != null)
            {
                return _sourceRepo;
            }

            _sourceRepo = new LocalSourceRepo(_sourceDirectory, _loggerFactory);
            return _sourceRepo;
        }

        private void CopyDirectories(string sourceDirectory, string destinationDirectory, bool recursive)
        {
            // Get the subdirectories for the specified directory.
            var sourceDir = new DirectoryInfo(sourceDirectory);
            if (!sourceDir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirectory);
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destinationDirectory))
            {
                _logger.LogDebug("Destination directory doesn't exist; creating it at {dstDir}", destinationDirectory);
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