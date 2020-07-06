// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Detector.DotNetCore;

namespace Microsoft.Oryx.Detector.Tests.DotNetCore
{
    class TestProjectFileProvider : DefaultProjectFileProvider
    {
        private readonly string _projectFilePath;

        public TestProjectFileProvider(string projectFilePath)
            : base(projectFileProviders: null)
        {
            _projectFilePath = projectFilePath;
        }

        public override string GetRelativePathToProjectFile(DetectorContext context)
        {
            return _projectFilePath;
        }
    }
}
