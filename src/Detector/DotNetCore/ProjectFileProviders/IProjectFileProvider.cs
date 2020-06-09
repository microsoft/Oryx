// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.Detector.DotNetCore
{
    public interface IProjectFileProvider
    {
        string GetRelativePathToProjectFile(RepositoryContext context);
    }
}
