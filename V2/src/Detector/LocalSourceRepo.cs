// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// Default implementation of <see cref="ISourceRepo"/> which is backed by the local file system.
    /// </summary>
    public class LocalSourceRepo : ISourceRepo
    {
        private readonly ILogger<LocalSourceRepo> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalSourceRepo"/> class.
        /// </summary>
        /// <param name="sourceDirectory">The directory containing the source code of the application.</param>
        public LocalSourceRepo(string sourceDirectory)
        {
            this.RootPath = sourceDirectory;
            this.logger = NullLogger<LocalSourceRepo>.Instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalSourceRepo"/> class.
        /// </summary>
        /// <param name="sourceDirectory">The directory containing the source code of the application.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public LocalSourceRepo(string sourceDirectory, ILoggerFactory loggerFactory)
        {
            this.RootPath = sourceDirectory;
            this.logger = loggerFactory.CreateLogger<LocalSourceRepo>();
        }

        /// <inheritdoc/>
        public string RootPath { get; }

        /// <inheritdoc/>
        public bool FileExists(params string[] paths)
        {
            var path = this.ResolvePath(paths);
            return File.Exists(path);
        }

        /// <inheritdoc/>
        public bool DirExists(params string[] paths)
        {
            var path = this.ResolvePath(paths);
            return Directory.Exists(path);
        }

        public IEnumerable<string> EnumerateFiles(
            string searchPattern,
            bool searchSubDirectories,
            params string[] subDirectoryToSearchUnder)
        {
            var directoryToSearchUnder = this.RootPath;
            if (subDirectoryToSearchUnder != null && subDirectoryToSearchUnder.Any())
            {
                directoryToSearchUnder = Path.Combine(directoryToSearchUnder, Path.Combine(subDirectoryToSearchUnder));
            }

            if (searchSubDirectories)
            {
                return directoryToSearchUnder.SafeEnumerateFiles(searchPattern);
            }

            return Directory.EnumerateFiles(directoryToSearchUnder, searchPattern);
        }

        /// <inheritdoc/>
        public string ReadFile(params string[] paths)
        {
            var path = this.ResolvePath(paths);
            return File.ReadAllText(path);
        }

        /// <inheritdoc/>
        public string[] ReadAllLines(params string[] paths)
        {
            var path = this.ResolvePath(paths);
            return File.ReadAllLines(path);
        }

        private string ResolvePath(params string[] paths)
        {
            var filePathInRepo = Path.Combine(paths);
            return Path.Combine(this.RootPath, filePathInRepo);
        }
    }
}
