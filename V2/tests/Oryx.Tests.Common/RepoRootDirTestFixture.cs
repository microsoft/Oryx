// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;

namespace Microsoft.Oryx.Tests.Common
{
    public class RepoRootDirTestFixture
    {
        public RepoRootDirTestFixture()
        {
            RepoRootDirPath = GetRepoRootDir();   
        }

        public string RepoRootDirPath { get; }

        private string GetRepoRootDir()
        {
            // Find repo root director so that we can get access to the 'platforms' directory
            string repoRootDir = null;
            var currentDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            while (true)
            {
                var oryxSlnFile = Path.Combine(currentDir.FullName, "Oryx.sln");
                if (File.Exists(oryxSlnFile))
                {
                    repoRootDir = currentDir.FullName;
                    break;
                }
                currentDir = currentDir.Parent;
            }

            if (repoRootDir == null)
            {
                throw new InvalidOperationException("Repo root could not be determined.");
            }

            return repoRootDir;
        }
    }
}
