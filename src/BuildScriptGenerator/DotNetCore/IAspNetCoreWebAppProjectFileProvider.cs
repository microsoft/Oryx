// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// Represents an abstraction which probes the source repository
    /// for project files which represent a web application
    /// </summary>
    public interface IAspNetCoreWebAppProjectFileProvider
    {
        string GetRelativePathToProjectFile(ISourceRepo sourceRepo);
    }
}
