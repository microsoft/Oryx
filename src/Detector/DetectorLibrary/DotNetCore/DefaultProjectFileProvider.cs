// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.Detector.DotNetCore
{
    public class DefaultProjectFileProvider
    {
        private readonly IEnumerable<IProjectFileProvider> _projectFileProviders;

        public DefaultProjectFileProvider(IEnumerable<IProjectFileProvider> projectFileProviders)
        {
            _projectFileProviders = projectFileProviders;
        }

        public virtual string GetRelativePathToProjectFile(RepositoryContext context)
        {
            foreach (var projectFileProvider in _projectFileProviders)
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
