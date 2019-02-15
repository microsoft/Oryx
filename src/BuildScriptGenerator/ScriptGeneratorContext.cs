// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public partial class ScriptGeneratorContext
    {
        public ISourceRepo SourceRepo { get; set; }

        /// <summary>
        /// Gets or sets the name of the main programming language used in the repo.
        /// If none is given, a language detection algorithm will attemp to detect it.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the version of the programming language used in the repo.
        /// If provided, the <see cref="ScriptGeneratorContext.Language"/> property should also be provided.
        /// </summary>
        public string LanguageVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether only the provided platform should be built, disabling
        /// the detection and build of all other platforms. If set to <c>true</c>, all other languages
        /// are disabled even if they are enabled by their specific flags.
        /// </summary>
        public bool DisableMultiPlatformBuild { get; set; } = false;

        /// <summary>
        /// Gets or sets specific properties for the build script.
        /// </summary>
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the detection and build of NodeJs code in the repo should be enabled.
        /// Defaults to true.
        /// </summary>
        public bool EnableNodeJs { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the detection and build of Python code in the repo should be enabled.
        /// Defaults to true.
        /// </summary>
        public bool EnablePython { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the detection and build of .NET core code in the repo should be enabled.
        /// Defaults to true.
        /// </summary>
        public bool EnableDotNetCore { get; set; } = true;

        /// <summary>
        /// Gets or sets the version of Python used in the repo.
        /// </summary>
        public string PythonVersion { get; set; }

        /// <summary>
        /// Gets or sets the version of Node used in the repo.
        /// </summary>
        public string NodeVersion { get; set; }

        /// <summary>
        /// Gets or sets the version of .NET Core used in the repo.
        /// </summary>
        public string DotnetCoreVersion { get; set; }
    }
}