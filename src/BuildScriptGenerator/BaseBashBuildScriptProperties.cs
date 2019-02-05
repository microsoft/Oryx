// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.Common.Utilities;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class BaseBashBuildScriptProperties
    {
        private string _preBuildScriptPath;
        private string _postBuildScriptPath;

        /// <summary>
        /// Gets or sets the collection of build script snippets.
        /// </summary>
        public IEnumerable<string> BuildScriptSnippets { get; set; }

        /// <summary>
        /// Gets or sets the path to the pre build script.
        /// </summary>
        public string PreBuildScriptPath
        {
            get => _preBuildScriptPath;
            set
            {
                _preBuildScriptPath = value;
                ProcessHelper.TrySetExecutableMode(value);
            }
        }

        /// <summary>
        /// Gets or sets the argument to the benv command.
        /// </summary>
        public string BenvArgs { get; set; }

        /// <summary>
        /// Gets or sets the path to the post build script.
        /// </summary>
        public string PostBuildScriptPath
        {
            get => _postBuildScriptPath;
            set
            {
                _postBuildScriptPath = value;
                ProcessHelper.TrySetExecutableMode(value);
            }
        }
    }
}