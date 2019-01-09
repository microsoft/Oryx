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
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<DefaultSourceRepoProvider> _logger;
        private bool _copiedToIntermediateDirectory = false;

        public DefaultSourceRepoProvider(
            ITempDirectoryProvider tempDirectoryProvider,
            IOptions<BuildScriptGeneratorOptions> options,
            ILoggerFactory loggerFactory)
        {
            _tempDirectoryProvider = tempDirectoryProvider;
            _options = options.Value;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<DefaultSourceRepoProvider>();
        }

        public ISourceRepo GetSourceRepo()
        {
            if (string.IsNullOrEmpty(_options.IntermediateDir))
            {
                _logger.LogDebug("Intermediate directory was not provided, so using source directory for build.");
                return new LocalSourceRepo(_options.SourceDir, _loggerFactory);
            }

            if (!_copiedToIntermediateDirectory)
            {
                PrepareIntermediateDirectory();

                _logger.LogDebug("Copying content from {srcDir} to {intermediateDir}", _options.SourceDir, _options.IntermediateDir);

                CopyDirectories(_options.SourceDir, _options.IntermediateDir, recursive: true);
                _copiedToIntermediateDirectory = true;
            }

            return new LocalSourceRepo(_options.IntermediateDir, _loggerFactory);
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

        private void PrepareIntermediateDirectory()
        {
            if (Directory.Exists(_options.IntermediateDir))
            {
                _logger.LogWarning("Intermediate directory {intermediateDir} already exists; deleting it", _options.IntermediateDir);
                Directory.Delete(_options.IntermediateDir, recursive: true);
            }

            _logger.LogDebug("Creating intermediate directory at {intermediateDir}", _options.IntermediateDir);
            Directory.CreateDirectory(_options.IntermediateDir);
        }
    }
}