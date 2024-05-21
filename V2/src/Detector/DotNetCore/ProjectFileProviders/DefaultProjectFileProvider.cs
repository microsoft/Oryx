// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.Detector.DotNetCore
{
    public class DefaultProjectFileProvider : IProjectFileProvider
    {
        private readonly IEnumerable<IProjectFileProvider> projectFileProviders;

        public DefaultProjectFileProvider(IEnumerable<IProjectFileProvider> projectFileProviders)
        {
            this.projectFileProviders = projectFileProviders;
        }

        public virtual string GetRelativePathToProjectFile(DetectorContext context)
        {
            foreach (var projectFileProvider in this.projectFileProviders)
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
