// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Golang
{
    public class GolangBashBuildSnippetProperties
    {
        public GolangBashBuildSnippetProperties(
            bool goModExists,
            string golangVersion)
        {
            this.GoModExists = goModExists;
            this.GolangVersion = golangVersion;
        }

        public bool GoModExists { get; set; }

        public string GolangVersion { get; set; }
    }
}
