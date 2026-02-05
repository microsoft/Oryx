// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Oryx.Common.Extensions
{
    /// <summary>
    /// A set of extension methods to help with operations involving the file system.
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Directories to skip during recursive file enumeration.
        /// These are typically dependency or build output directories that don't help with platform detection.
        /// </summary>
        private static readonly HashSet<string> DirectoriesToSkip = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "node_modules",
            ".git",
            "__pycache__",
        };

        /// <summary>
        /// This method will write the given contents to the path provided and create any non-existent directories
        /// along the path since File.WriteAllText will throw if one of the directories in the path does not exist.
        /// </summary>
        /// <param name="outputPath">The file path to write the contents to.</param>
        /// <param name="contents">The contents to be written to the given file path.</param>
        public static void SafeWriteAllText(this string outputPath, string contents)
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                return;
            }

            outputPath = Path.GetFullPath(outputPath).TrimEnd('/').TrimEnd('\\');
            var parentPath = Directory.GetParent(outputPath).FullName;
            if (!Directory.Exists(parentPath))
            {
                _ = Directory.CreateDirectory(parentPath);
            }

            File.WriteAllText(outputPath, contents);
        }

        /// <summary>
        /// This method will recursively enumerate the files under a given path since the Directory.EnumerateFiles
        /// call does not check to see if a directory exists, or if the directory is accessible, before
        /// enumerating it.
        /// </summary>
        /// <param name="path">The directory to recursively enumerate the files in.</param>
        /// <param name="searchPattern">The search string to match against the names of files in path.</param>
        /// <returns>All files that are accessible under the given directory.</returns>
        public static IEnumerable<string> SafeEnumerateFiles(this string path, string searchPattern)
        {
            var fileResult = Directory.EnumerateFiles(path, searchPattern);
            var directoryResult = Directory.EnumerateDirectories(path)
                                            .Where(d => Directory.Exists(d) && !DirectoriesToSkip.Contains(Path.GetFileName(d)))
                                            .SelectMany(d => d.SafeEnumerateFiles(searchPattern));
            return fileResult.Concat(directoryResult);
        }
    }
}
