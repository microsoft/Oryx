// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.DotnetCore
{
    /// <summary>
    /// Represents an abstraction which probes the source repository
    /// for project files which represent a web application
    /// </summary>
    public interface IAspNetCoreWebAppProjectFileProvider
    {
        string GetProjectFile(ISourceRepo sourceRepo);
    }
}
