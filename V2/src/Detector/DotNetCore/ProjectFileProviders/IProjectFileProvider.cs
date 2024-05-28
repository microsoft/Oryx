// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector.DotNetCore
{
    public interface IProjectFileProvider
    {
        string GetRelativePathToProjectFile(DetectorContext context);
    }
}
