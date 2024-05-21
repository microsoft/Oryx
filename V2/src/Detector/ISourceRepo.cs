// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// Abstracts a source code repository.
    /// </summary>
    public interface ISourceRepo
    {
        /// <summary>
        /// Gets the path to the root of the repository in the file system.
        /// </summary>
        string RootPath { get; }

        /// <summary>
        /// Check whether a file (not including directories) exists in the source repo.
        /// </summary>
        /// <param name="paths">The path to the file inside the source repo.</param>
        /// <returns>true if the file exists, false otherwise.</returns>
        bool FileExists(params string[] paths);

        /// <summary>
        /// Check whether a directory exists in the source repo.
        /// </summary>
        /// <param name="paths">The path to the directory inside the source repo.</param>
        /// <returns>true if the directory exists, false otherwise.</returns>
        bool DirExists(params string[] paths);

        /// <summary>
        /// Reads a file from the source repo.
        /// </summary>
        /// <param name="paths">Path to the file inside the repo.</param>
        /// <returns>The content of the file.</returns>
        string ReadFile(params string[] paths);

        /// <summary>
        /// Reads all lines in a file from the source repo.
        /// </summary>
        /// <param name="paths">Path to the file inside the repo.</param>
        /// <returns>An array of lines from the file.</returns>
        string[] ReadAllLines(params string[] paths);

        /// <summary>
        /// Gets a list of paths to files based on the specified <paramref name="searchPattern"/>.
        /// </summary>
        /// <param name="searchPattern">The search pattern of the file name.</param>
        /// <param name="searchSubDirectories">
        /// true files from subdirectories should be included, false for only the files in the root.
        /// </param>
        /// <param name="subDirectoryToSearchUnder">
        /// Sub directory under the root under which the search needs to happen.
        /// </param>
        /// <returns>A collection of file paths.</returns>
        IEnumerable<string> EnumerateFiles(
            string searchPattern,
            bool searchSubDirectories,
            params string[] subDirectoryToSearchUnder);
    }
}
