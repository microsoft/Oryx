// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.DotNetCore
{
    class TestProjectFileProvider : DefaultProjectFileProvider
    {
        private readonly string _projectFilePath;

        public TestProjectFileProvider(string projectFilePath)
            : base(projectFileProviders: null)
        {
            _projectFilePath = projectFilePath;
        }

        public override string GetRelativePathToProjectFile(RepositoryContext context)
        {
            return _projectFilePath;
        }
    }
}
