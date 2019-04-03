// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class BaseBashBuildScriptProperties
    {
        public const string PreBuildScriptPrologue = "Executing pre-build script...";
        public const string PreBuildScriptEpilogue = "Finished executing pre-build script.";

        public const string PostBuildScriptPrologue = "Executing post-build script...";
        public const string PostBuildScriptEpilogue = "Finished executing post-build script.";

        /// <summary>
        /// Gets or sets the collection of build script snippets.
        /// </summary>
        public IEnumerable<string> BuildScriptSnippets { get; set; }

        /// <summary>
        /// Gets or sets the path to the pre build script.
        /// </summary>
        public string PreBuildScriptPath { get; set; }

        /// <summary>
        /// Gets or sets the argument to the benv command.
        /// </summary>
        public string BenvArgs { get; set; }

        /// <summary>
        /// Gets or sets the path to the post build script.
        /// </summary>
        public string PostBuildScriptPath { get; set; }

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