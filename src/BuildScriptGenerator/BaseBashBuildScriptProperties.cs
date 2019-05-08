// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class BaseBashBuildScriptProperties
    {
        public const string PreBuildCommandPrologue = "Executing pre-build command...";
        public const string PreBuildCommandEpilogue = "Finished executing pre-build command.";

        public const string PostBuildCommandPrologue = "Executing post-build command...";
        public const string PostBuildCommandEpilogue = "Finished executing post-build command.";

        /// <summary>
        /// Gets or sets the collection of build script snippets.
        /// </summary>
        public IEnumerable<string> BuildScriptSnippets { get; set; }

        /// <summary>
        /// Gets or sets the the pre build script content
        /// </summary>
        public string PreBuildCommand { get; set; }

        /// <summary>
        /// Gets or sets the argument to the benv command.
        /// </summary>
        public string BenvArgs { get; set; }

        /// <summary>
        /// Gets or sets the path to the post build script content.
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
    }
}