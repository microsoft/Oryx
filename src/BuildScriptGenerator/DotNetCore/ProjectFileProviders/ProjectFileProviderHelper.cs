// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class ProjectFileProviderHelper
    {
        public static string GetRelativePathToProjectFile(
            IEnumerable<IProjectFileProvider> projectFileProviders,
            BuildScriptGeneratorContext context)
        {
            foreach (var projectFileProvider in projectFileProviders)
            {
                var projectFile = projectFileProvider.GetRelativePathToProjectFile(context);
                if (!string.IsNullOrEmpty(projectFile))
                {
                    return projectFile;
                }
            }

            return null;
        }
    }
}
