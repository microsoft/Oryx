// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Abstracts a source code repository.
    /// </summary>
    public interface ISourceRepo
    {
        /// <summary>
        /// The path to the root of the repository in the file system.
        /// </summary>
        string RootPath { get; }

        /// <summary>
        /// Check whether a file exists in the source repo.
        /// </summary>
        /// <param name="paths">The path to the file inside the source repo.</param>
        /// <returns>true if the file exists, false otherwise.</returns>
        bool FileExists(params string[] paths);

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
        /// <returns>A list of lines from the file.</returns>
        string[] ReadAllLines(params string[] paths);

        /// <summary>
        /// Gets a list of paths to files based on the specified <paramref name="searchPattern"/>.
        /// </summary>
        /// <param name="searchPattern">The search pattern of the file name.</param>
        /// <param name="searchSubDirectories"></param>
        /// <returns>A collection of file paths.</returns>
        IEnumerable<string> EnumerateFiles(string searchPattern, bool searchSubDirectories);
    }
}