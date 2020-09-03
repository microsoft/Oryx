// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Ruby
{
    /// <summary>
    /// Build script template for Ruby in Bash.
    /// </summary>
    internal class RubyBashBuildSnippetProperties
    {
        public bool HasRailsDependencies { get; set; }

        public bool UseBundlerToInstallDependencies { get; set; }

        public bool RunRakeExecuteCommand { get; set; }
    }
} 