﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class BaseBashBuildScriptProperties
    {
        public string PreBuildCommandPrologue { get; set; } = Constants.PreBuildCommandPrologue;

        public string PreBuildCommandEpilogue { get; set; } = Constants.PreBuildCommandEpilogue;

        public string PostBuildCommandPrologue { get; set; } = Constants.PostBuildCommandPrologue;

        public string PostBuildCommandEpilogue { get; set; } = Constants.PostBuildCommandEpilogue;

        /// <summary>
        /// Gets or sets the collection of build script snippets.
        /// </summary>
        public IEnumerable<string> BuildScriptSnippets { get; set; }

        /// <summary>
        /// Gets or sets a list of OS packages that should be installed for this build.
        /// </summary>
        [NotNull]
        public string[] OsPackagesToInstall { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the the pre-build script content.
        /// </summary>
        public string PreBuildCommand { get; set; }

        /// <summary>
        /// Gets or sets the argument to the benv command.
        /// </summary>
        public string BenvArgs { get; set; }

        /// <summary>
        /// Gets or sets the path to the post-build script content.
        /// </summary>
        public string PostBuildCommand { get; set; }

        public IEnumerable<string> DirectoriesToExcludeFromCopyToBuildOutputDir { get; set; }

        public IEnumerable<string> DirectoriesToExcludeFromCopyToIntermediateDir { get; set; }

        /// <summary>
        /// Gets or sets a list of properties for the build. Those properties are stored in a
        /// manifest file that can be used when running the app.
        /// </summary>
        public IDictionary<string, string> BuildProperties { get; set; }

        /// <summary>
        /// Gets or sets the name of the manifest file.
        /// </summary>
        public string ManifestFileName { get; set; }

        /// <summary>
        /// Gets or sets the path to the directory where the manifest file needs to be put.
        /// </summary>
        public string ManifestDir { get; set; }

        /// <summary>
        /// Gets or sets the file name where build commands will be dynamically written during build.
        /// </summary>
        public string BuildCommandsFileName { get; set; }

        /// <summary>
        /// Gets or sets the path to benv file.
        /// </summary>
        public string BenvPath { get; set; }

        /// <summary>
        /// Gets or sets the path to logger file.
        /// </summary>
        public string LoggerPath { get; set; }

        /// <summary>
        /// Gets or sets the bash script which install the platform binaries.
        /// </summary>
        public string PlatformInstallationScript { get; set; }

        /// <summary>
        /// Gets a value indicating whether the output directory is a nested directory of the source.
        /// </summary>
        public bool OutputDirectoryIsNested { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether the source directory's content must be copied to the destination
        /// directory. <see cref="IProgrammingPlatform"/> set this flag when generating build script.
        /// <see cref="DotNetCore.DotNetCorePlatform"/> sets this as <c>false</c> since we do not want to copy source
        /// files like '.cs' files to destination directory where as in other platforms this is fine to do.
        /// </summary>
        public bool CopySourceDirectoryContentToDestinationDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entire output directory (excluding the manifest file) needs
        /// to be compressed.
        /// </summary>
        public bool CompressDestinationDir { get; set; }
    }
}
