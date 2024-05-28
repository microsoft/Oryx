// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class LocalSourceRepo : ISourceRepo
    {
        private readonly ILogger<LocalSourceRepo> logger;

        public LocalSourceRepo(string sourceDirectory)
        {
            this.RootPath = sourceDirectory;
            this.logger = NullLogger<LocalSourceRepo>.Instance;
        }

        public LocalSourceRepo(string sourceDirectory, ILoggerFactory loggerFactory)
        {
            this.RootPath = sourceDirectory;
            this.logger = loggerFactory.CreateLogger<LocalSourceRepo>();
        }

        public string RootPath { get; }

        public bool FileExists(params string[] paths)
        {
            var path = this.ResolvePath(paths);
            return File.Exists(path);
        }

        public bool DirExists(params string[] paths)
        {
            var path = this.ResolvePath(paths);
            return Directory.Exists(path);
        }

        public IEnumerable<string> EnumerateFiles(string searchPattern, bool searchSubDirectories)
        {
            if (searchSubDirectories)
            {
                return this.RootPath.SafeEnumerateFiles(searchPattern);
            }

            return Directory.EnumerateFiles(this.RootPath, searchPattern);
        }

        public string ReadFile(params string[] paths)
        {
            var path = this.ResolvePath(paths);
            return File.ReadAllText(path);
        }

        public string[] ReadAllLines(params string[] paths)
        {
            var path = this.ResolvePath(paths);
            return File.ReadAllLines(path);
        }

        public string GetGitCommitId()
        {
            var exitCode = 1;
            var output = string.Empty;
            var error = string.Empty;

            try
            {
                (exitCode, output, error) = ProcessHelper.RunProcess(
                    "git",
                    new string[] { "rev-parse", "HEAD" },
                    this.RootPath,
                    TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                // Ignore any exceptions as we do not want them to bubble up to end user as this functionality
                // is not required for the build to work.
                this.logger.LogError(ex, "An error occurred while trying to get commit ID from repo");
                return null;
            }

            if (exitCode != 0)
            {
                this.logger.LogWarning(
                    "Could not get commit ID from repo. " +
                    "Exit code: {exitCode}, Output: {stdOut}, Error: {stdErr}",
                    exitCode,
                    output,
                    error);
                return null;
            }

            return output?.Trim();
        }

        private string ResolvePath(params string[] paths)
        {
            var filePathInRepo = Path.Combine(paths);
            return Path.Combine(this.RootPath, filePathInRepo);
        }
    }
}